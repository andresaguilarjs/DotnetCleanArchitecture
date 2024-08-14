using Microsoft.Extensions.DependencyInjection;
using WebApi.Abstractions;

namespace WebApi
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            services.AddControllers();
            return services;
        }
    }
}