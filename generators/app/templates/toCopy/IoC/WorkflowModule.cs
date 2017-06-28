using Interfaces.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Workflows;

namespace IoC
{
    public static class WorkflowModule
    {
        public static IServiceCollection AddDomainWorkflows(this IServiceCollection services)
        {
            // Transient: A new instance of the type is used every time the type is requested.
            // Scoped:    A new instance of the type is created the first time it’s requested within a 
            //            given HTTP request, and then re - used for all subsequent types resolved during that HTTP request.
            // Singleton: A single instance of the type is created once, and used by all subsequent requests for that type.

            // Add application workflows
            //services.AddScoped<IThingWorkflow, ThingWorkflow>();

            return services;
        }
    }
}
