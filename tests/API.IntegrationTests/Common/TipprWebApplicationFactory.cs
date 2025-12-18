using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Serilog;

namespace API.IntegrationTests.Common;

public sealed class TipprWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Serilog diagnostic context (så middleware kan aktiveras i tests)
            services.TryAddSingleton<Serilog.IDiagnosticContext>(
                new Serilog.Extensions.Hosting.DiagnosticContext(
                    new LoggerConfiguration().CreateLogger()
                )
            );

            // ✅ KRITISKT: rensa bort auth-options som Program.cs registrerar
            services.RemoveAll<IConfigureOptions<AuthenticationOptions>>();
            services.RemoveAll<IPostConfigureOptions<AuthenticationOptions>>();

            // ✅ Registrera endast SupabaseAuth i tests, men med TestAuthHandler
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "SupabaseAuth";
                options.DefaultChallengeScheme = "SupabaseAuth";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("SupabaseAuth", _ => { });

            // --- SQLite in-memory shared connection ---
            services.RemoveAll<DbContextOptions<TipprDbContext>>();

            _connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
            _connection.Open();

            services.AddDbContext<TipprDbContext>(opt =>
            {
                opt.UseSqlite(_connection);
                opt.EnableDetailedErrors();
                opt.EnableSensitiveDataLogging();
            });

            // Registera ITipprDbContext mot samma SQLite-context
            services.RemoveAll<Application.Common.Interfaces.ITipprDbContext>();
            services.AddScoped<Application.Common.Interfaces.ITipprDbContext>(sp =>
                sp.GetRequiredService<TipprDbContext>());

            // Skapa schema
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TipprDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
        }
    }
}
