using BlazorObservers.ObserverLibrary.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorObservers.ObserverLibrary.DI
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddResizeObserverRegistrationService(this IServiceCollection services)
        {
            return services.AddScoped<ResizeObserverRegistrationService>();
        }
    }
}
