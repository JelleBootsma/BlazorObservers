using BlazorObservers.ObserverLibrary.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorObservers.ObserverLibrary.DI
{
    /// <summary>
    /// Contains shorthand methods to register Observer services
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Register scoped observer services in the dependency injection
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddResizeObserverService(this IServiceCollection services)
        {
            return services
                .AddScoped<ResizeObserverService>()
                .AddScoped<IntersectionObserverService>();
        }
    }
}
