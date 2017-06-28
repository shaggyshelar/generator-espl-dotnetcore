using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Data;
using Interfaces;
using Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Models.Domain;
using System.Linq.Dynamic.Core;

namespace Services
{
    public abstract class CrudService<TModel> : BaseService<CrudService<TModel>>, ICrudService<TModel> where TModel : BaseModel
    {
        protected readonly ApplicationDbContext DbContext;
        protected readonly DbSet<TModel> Models;

        public CrudService(IApplicationDbContext dbContext)
        {
            DbContext = dbContext as ApplicationDbContext;

            if (DbContext != null)
                Models = DbContext.Set<TModel>();
        }

        // Each service will need to set up the Where clause to be used when searching and getting count
        protected abstract Expression<Func<TModel, bool>> GetQueryPredicate(string searchTerm = "");

        // This method needs to be implemented in each service to map the modified fields to the
        // original fields when an update action occurs. This allows us to exclude fields that are
        // set by the system and should not be modified by the user.
        protected abstract TModel GetUpdatedOriginal(TModel original, TModel modified);
        
        public IEnumerable<TModel> Get(uint pageNumber, uint pageSize, string searchTerm = "", string sortBy = "", bool sortDesc = false, bool archives = false)
        {
            var models = Models.AsQueryable();

			models = archives ? models.Where(x => x.ArchivedOn != null) : models.Where(x => x.ArchivedOn == null);
			
            // If a search term was passed in, apply the predicate where clause
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                var predicate = GetQueryPredicate(searchTerm);
                models = models.Where(predicate);
            }

            return GetQueryPage(pageNumber, pageSize, sortBy, sortDesc, models);
        }
        
        public TModel Get(long id)
        {
            return Models.FirstOrDefault(x => x.Id == id);
        }
        
        public TModel Store(TModel model, string applicationUserId)
        {
            var transactionType = DomainEnums.TransactionTypes.None;

            if (model.IsNew)
            {
                // We're going to use the map method to strip the new model of
                // fields that the user cannot edit
                // Create a new model as the original
                var original = (TModel) Activator.CreateInstance(typeof(TModel));
                model = this.GetUpdatedOriginal(original, model);

                model.CreatedOn = DateTime.Now;
                model.CreatedBy = applicationUserId;
                DbContext.Entry(model).State = EntityState.Added;
                transactionType = DomainEnums.TransactionTypes.Insert;
            }
            else
            {
                // Map the updated entity to the saved entity
                // This ensures that the front-end isn't overwriting fields
                // it shouldn't be.
                var original = Models.FirstOrDefault(x => x.Id == model.Id);
                model = this.GetUpdatedOriginal(original, model);

                model.ModifiedOn = DateTime.Now;
                model.ModifiedBy = applicationUserId;
                DbContext.Entry(model).State = EntityState.Modified;
                transactionType = DomainEnums.TransactionTypes.Update;
            }

            DbContext.SaveChanges();

            StoreHistory(model, applicationUserId, transactionType);

            return model;
        }

        public void Delete(long id, string applicationUserId)
        {
            var model = Get(id);

            if (model != null) {
                DbContext.Remove(model);
                DbContext.SaveChanges();
            }

            StoreHistory(model, applicationUserId, DomainEnums.TransactionTypes.Delete);
        }

        public void Archive(long id, string applicationUserId)
        {
            var model = Get(id);

            if (model != null)
            {
                model.ArchivedBy = applicationUserId;
                model.ArchivedOn = DateTime.Now;

                DbContext.SaveChanges();
            }

            StoreHistory(model, applicationUserId, DomainEnums.TransactionTypes.Delete);
        }

        public int Count(string searchTerm = "", bool archives = false)
        {
            var predicate = GetQueryPredicate(searchTerm);

			var models = archives ? Models.Where(x => x.ArchivedOn != null) : Models.Where(x => x.ArchivedOn == null);

            return (string.IsNullOrWhiteSpace(searchTerm))
                ? models.AsQueryable().Count()
                : models.Where(predicate).Count();
        }

        // This method allows you to build the full query and pass that in to get a page
        protected IEnumerable<TModel> GetQueryPage(uint pageNumber, uint pageSize, string sortBy, bool sortDesc, IQueryable<TModel> query)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                // Default sort is edited descending
                query = query
                    .OrderByDescending(x => (x.ModifiedOn != null) ? x.ModifiedOn : x.CreatedOn);
            }
            else
            {
                // Otherwise, dynamically sort by
                var sortType = sortDesc ? "DESC" : "ASC";
                var dynamicSort = $"{sortBy} {sortType}";

                query = query.OrderBy(dynamicSort);

            }
            
            return query
                .Skip(Convert.ToInt32(pageSize) * Convert.ToInt32(pageNumber))
                .Take(Convert.ToInt32(pageSize));
        }

        private void StoreHistory(
            TModel model, 
            string applicationUserId,
            DomainEnums.TransactionTypes tranactionType)
        {
            var modelType = typeof(TModel);
            // Attempt to get the interface type that tell us this model has a history table
            // associated with it
            var storeHistoryType = modelType
                .GetInterfaces()
                .FirstOrDefault(x => x.AssemblyQualifiedName.Contains("IStoreHistory"));
            
            // If the history type isn't null, and the model inherits from it...
            if (storeHistoryType != null && storeHistoryType.IsAssignableFrom(modelType))
            {
                // We have a valid interface for history inserts
                // Get the generic method that will get us the new history model
                var convertMethod = storeHistoryType.GetMethod("Convert");
                // Get the history model
                var historyModel = convertMethod.Invoke(model, new object[] { model, applicationUserId, tranactionType });
                // Insert the history model :)
                DbContext.Entry(historyModel).State = EntityState.Added;
                DbContext.SaveChanges();
            }
        }
    }
}
