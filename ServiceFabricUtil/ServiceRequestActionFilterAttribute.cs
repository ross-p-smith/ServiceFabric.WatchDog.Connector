namespace ServiceFabric.Helpers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// This attribute has been written this way so that we can create a ServiceRequestFilter
    /// by injecting in an instance of the IServiceEventSource. You can now decorate a Controller 
    /// with this attribute so that every controller request is written to ETW
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,
        Inherited = true,
        AllowMultiple = false)]
    [CLSCompliant(false)]
    public sealed class ServiceRequestActionFilterAttribute : TypeFilterAttribute
    {
        public ServiceRequestActionFilterAttribute() 
            : base(typeof(ServiceRequestFilter))
        {
        }

        /// <summary>
        /// This nested class assumes that an instance of IServiceEventSource is wired up
        /// in the container.
        /// </summary>
        /// <see cref="https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters#dependency-injection"/>
        private class ServiceRequestFilter : IAsyncActionFilter
        {
            private readonly IServiceEventSource _eventSource;
            public ServiceRequestFilter(IServiceEventSource eventSource)
            {
                this._eventSource = eventSource;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                // Raise the event that the service request has started.
                _eventSource.ServiceRequestStart(context.ActionDescriptor.DisplayName);

                string stopMessage = string.Empty;
                try
                {
                    await next();
                }
                catch (Exception exception)
                {
                    stopMessage = exception.Message;
                }

                _eventSource.ServiceRequestStop(context.ActionDescriptor.DisplayName, stopMessage);
            }
        }
    }
}
