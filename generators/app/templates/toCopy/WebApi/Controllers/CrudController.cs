using System;
using Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.Domain;
using WebApi.Models;

namespace WebApi.Controllers
{
    public abstract class CrudController<TController, TModel, TService> : BaseController<TController> 
        where TModel : BaseModel 
        where TService : ICrudService<TModel>
    {
        protected readonly TService Service;
        private readonly bool shouldLog = true;

        public CrudController(
            IServiceProvider services,
            TService service) : base(services)
        {
            Service = service;

            // If this is the LogEntry controller, don't log stuff
            shouldLog = (typeof(TController) != typeof(LogEntryController));
        }

        [HttpGet("{pageNumber}/{pageSize}")]
        public PageResultViewModel<TModel> Get(
            uint pageNumber,
            uint pageSize,
            [FromQuery] string searchTerm = "",
            [FromQuery] string sortBy = "",
            [FromQuery] bool sortDesc = false,
			[FromQuery] bool archives = false)
        {
            Log($"Get: pageNumber-{pageNumber}, pageSize-{pageSize}, searchTerm-{searchTerm}, sortBy-{sortBy}, archives-{archives}");

            var pageResults = new PageResultViewModel<TModel>
            {
                Results = Service.Get(pageNumber, pageSize, searchTerm, sortBy, sortDesc, archives),
                Total = Service.Count(searchTerm, archives),
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return pageResults;
        }

        [HttpGet("{id}")]
        public TModel Get(long id)
        {
            Log($"Get: id-{id}");
            return Service.Get(id);
        }

        [HttpPost]
        public virtual IActionResult Post([FromBody] TModel model)
        {
            Log($"Post");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(Service.Store(model, CurrentUser.Id));
        }

        [HttpPut("{id}")]
        public  virtual IActionResult Put(long id, [FromBody] TModel model)
        {
            Log($"Put: id-{model.Id}");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(Service.Store(model, CurrentUser.Id));
        }

        [HttpDelete("{id}")]
        public void Delete(long id)
        {
            Log($"Delete: id-{id}");
            Service.Delete(id, CurrentUser.Id);
        }

        [HttpPut("archive/{id}")]
        public void Archive(long id)
        {
            Log($"Archive: id-{id}");
            Service.Archive(id, CurrentUser.Id);
        }

        private void Log(string message)
        {
            if (shouldLog)
            {
                Logger.LogInformation(message);
            }
        }
    }
}