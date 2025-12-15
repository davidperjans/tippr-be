using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DATABASE_CONNECTION");

            // Register DbContext - connection string will be validated at runtime
            services.AddDbContext<TipprDbContext>(options =>
            {
                // Skip configuration if no connection string (allows EF tools to work)
                if (!string.IsNullOrEmpty(connectionString))
                {
                    options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorCodesToAdd: null);

                        npgsqlOptions.CommandTimeout(30);
                    });

                    // Enable sensitive data logging in development
                    #if DEBUG
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                    #endif
                }
            });

            return services;
        }
    }
}