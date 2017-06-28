using System.Collections.Generic;
using Models.Domain;

namespace Interfaces.Services
{
    public interface ICrudService<TModel> where TModel : BaseModel
    {
        IEnumerable<TModel> Get(
            uint pageNumber,
            uint pageSize,
            string searchTerm = "",
            string sortBy = "",
            bool sortDesc = false,
			bool archives = false);
        TModel Get(long id);
        TModel Store(TModel model, string applicationUserId);
        void Delete(long id, string applicationUserId);
        void Archive(long id, string applicationUserId);
        int Count(string searchTerm = "", bool archives = false);
    }
}
