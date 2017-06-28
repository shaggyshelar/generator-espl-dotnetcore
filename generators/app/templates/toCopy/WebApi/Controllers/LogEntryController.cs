using System;
using Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Domain;

namespace WebApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LogEntryController : CrudController<LogEntryController, LogEntry, ILogEntryService>
    {
        // NOTE: Do NOT add logging to this controller! :)
        
        public LogEntryController(
            IServiceProvider services,
            ILogEntryService logEntries)
            : base(services, logEntries)
        {
        }
        
        [Authorize(Roles = "Developer")]
        [HttpPost("clearLogs")]
        public ClearLogModel ClearLogs([FromBody] ClearLogModel model)
        {
            Service.ClearLogs(model.Type);
            return model;
        }

        // This is a hack. All I need is a string, but posting a single value doesn't get bound. ~Nick
        public class ClearLogModel
        {
            public string Type { get; set; }
        }
    }
}
