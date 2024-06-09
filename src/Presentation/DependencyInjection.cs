using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Presentation.Abstractions;

namespace Presentation
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            services.AddMvc()
                .AddApplicationPart(typeof(ApiController).Assembly)
                .AddControllersAsServices();
            return services;
        }
    }
}
