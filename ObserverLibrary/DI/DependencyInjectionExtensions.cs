using BlazorObservers.ObserverLibrary.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorObservers.ObserverLibrary.DI
{
    /// <summary>
    /// Contains shorthand methods to register ObserverRegistrationServices
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Register a scoped ResizeObserverRegistrationService in the dependency injection
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddResizeObserverRegistrationService(this IServiceCollection services)
        {
            return services.AddScoped<ResizeObserverRegistrationService>();
        }
    }
}
