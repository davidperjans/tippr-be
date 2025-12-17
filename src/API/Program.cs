using API.Auth;
using Application;
using Infrastructure;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog Setup - Använd Configuration-baserad setup för flexibilitet
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

try
{
    Log.Information("Starting Tippr API");

    // 2. Add Services (DI Container)
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddMemoryCache();

    // Layers
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApplicationServices();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(builder.Configuration["AllowedOrigins"] ?? "http://localhost:5173")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    // Swagger
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tippr API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste Supabase access_token"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    // Authentication & Authorization
    builder.Services
        .AddAuthentication("SupabaseAuth")
        .AddScheme<SupabaseAuthenticationOptions, SupabaseAuthenticationHandler>("SupabaseAuth", _ => { });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.Requirements.Add(new AdminRequirement()));
    });
    builder.Services.AddScoped<IAuthorizationHandler, AdminRequirementHandler>();

    // 3. Build & Pipeline
    var app = builder.Build();

    // Serilog Request Logging (Körs alltid, men konfigureras via appsettings)
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            var user = httpContext.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserId", user.FindFirst("user_id")?.Value);
            }
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");

    app.UseAuthentication();
    app.UseMiddleware<UserSyncMiddleware>();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Tippr API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Gör Program-klassen synlig för Integration Tests
public partial class Program { }