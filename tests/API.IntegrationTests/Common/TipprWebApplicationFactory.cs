using Application.Common.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.IntegrationTests.Common;

public class TipprWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public TipprWebApplicationFactory()
    {
        // Vi använder en delad cache för att databasen ska överleva mellan olika DbContext-instanser i samma test
        _connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // 1. Hantera Serilog IDiagnosticContext kraschen
            services.RemoveAll<Serilog.IDiagnosticContext>();
            services.AddSingleton<Serilog.IDiagnosticContext, FakeDiagnosticContext>();

            // 2. Ersätt Databas-konfigurationen
            services.RemoveAll<DbContextOptions<TipprDbContext>>();
            services.RemoveAll<TipprDbContext>();
            services.RemoveAll<ITipprDbContext>();

            services.AddDbContext<TipprDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            services.AddScoped<ITipprDbContext>(sp => sp.GetRequiredService<TipprDbContext>());

            // 3. Konfigurera Test-Autentisering
            // Vi tar bort den befintliga autentiseringen för att tvinga in vår Test-variant
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthenticationHandler.TestScheme;
                options.DefaultScheme = TestAuthenticationHandler.TestScheme;
                options.DefaultChallengeScheme = TestAuthenticationHandler.TestScheme;
            })
            .AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>(
                TestAuthenticationHandler.TestScheme, options => { });

            // 4. Initiera Databasen
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TipprDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }
        base.Dispose(disposing);
    }
}

public class FakeDiagnosticContext : Serilog.IDiagnosticContext
{
    public void Set(string propertyName, object value, bool destructureObjects = false) { }
    public void SetException(Exception exception) { }
}