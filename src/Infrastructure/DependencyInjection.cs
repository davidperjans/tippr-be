using Application.Common.Interfaces;
using Infrastructure.Auth;
using Infrastructure.Data;
using Infrastructure.External.ApiFootball;
using Infrastructure.Services;
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

            var supabaseUrl = configuration["Supabase:Url"];
            var supabaseKey = configuration["Supabase:Key"];

            var isTesting = configuration["ASPNETCORE_ENVIRONMENT"] == "Testing";

            if (!isTesting && (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey)))
            {
                throw new InvalidOperationException("Supabase:Url or Key is not configured");
            }

            services.AddScoped<Supabase.Client>(_ =>
                new Supabase.Client(
                    supabaseUrl ?? "https://fake.url",
                    supabaseKey ?? "fake-key",
                    new SupabaseOptions { AutoRefreshToken = false }
                )
            );

            services.Configure<SupabaseStorageOptions>(opts =>
            {
                opts.Url = supabaseUrl ?? "";
                opts.ServiceKey = supabaseKey ?? "";
            });

            // HttpContextAccessor
            services.AddHttpContextAccessor();

            services.AddHttpClient<IAvatarStorage, SupabaseAvatarStorage>();

            // Services
            services.AddScoped<ITipprDbContext>(sp =>
                sp.GetRequiredService<TipprDbContext>());

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<IPointsCalculator, PointsCalculator>();
            services.AddScoped<IStandingsService, StandingsService>();

            // API-FOOTBALL Client
            services.Configure<ApiFootballOptions>(options =>
            {
                options.BaseUrl = configuration["ApiFootball:BaseUrl"] ?? "https://v3.football.api-sports.io";
                options.ApiKey = configuration["ApiFootball:ApiKey"] ?? string.Empty;
            });

            services.AddHttpClient<IApiFootballClient, ApiFootballClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            return services;
        }
    }
}