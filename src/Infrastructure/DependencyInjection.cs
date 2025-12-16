using Application.Common.Interfaces;
using Infrastructure.Auth;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Supabase;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

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

            var supabaseUrl = configuration["Supabase:Url"]
                ?? throw new InvalidOperationException("Supabase:Url is not configured");
            var supabaseKey = configuration["Supabase:Key"]
                ?? throw new InvalidOperationException("Supabase:Key is not configured");

            services.AddScoped<Supabase.Client>(_ =>
                new Supabase.Client(
                    supabaseUrl,
                    supabaseKey,
                    new SupabaseOptions
                    {
                        AutoRefreshToken = false,
                        AutoConnectRealtime = false
                    }
                )
            );

            // HttpContextAccessor
            services.AddHttpContextAccessor();


            // Services
            services.AddScoped<ITipprDbContext>(sp =>
                sp.GetRequiredService<TipprDbContext>());

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICurrentUser, CurrentUser>();

            return services;
        }
    }
}