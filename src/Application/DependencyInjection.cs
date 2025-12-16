using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddValidatorsFromAssembly(assembly);
            services.AddAutoMapper(assembly);
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            return services;
        }
    }
}
