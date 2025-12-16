using Application;
using Infrastructure;
using Infrastructure.Auth;
using Microsoft.OpenApi.Models;
using Serilog;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting Tippr API");

                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId()
                    .Enrich.WithProperty("Application", "Tippr")
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName));

                // Services
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
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
                        Description = "Paste Supabase access_token here (without 'Bearer ')"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                });

                // Memory cache
                builder.Services.AddMemoryCache();

                // Layers
                builder.Services.AddInfrastructureServices(builder.Configuration);
                builder.Services.AddApplicationServices();


                // Supabase Authentication
                builder.Services
                    .AddAuthentication("SupabaseAuth")
                    .AddScheme<SupabaseAuthenticationOptions, SupabaseAuthenticationHandler>(
                        "SupabaseAuth",
                        options => { });

                builder.Services.AddAuthorization();

                var app = builder.Build();

                app.UseSerilogRequestLogging(options =>
                {
                    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                    {
                        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());

                        // Lägg till user info om autentiserad
                        if (httpContext.User.Identity?.IsAuthenticated == true)
                        {
                            var userId = httpContext.User.FindFirst("user_id")?.Value;
                            var email = httpContext.User.FindFirst("email")?.Value;

                            if (!string.IsNullOrEmpty(userId))
                                diagnosticContext.Set("UserId", userId);
                            if (!string.IsNullOrEmpty(email))
                                diagnosticContext.Set("UserEmail", email);
                        }
                    };
                });

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                // Middleware order
                app.UseAuthentication();
                app.UseMiddleware<UserSyncMiddleware>();
                app.UseAuthorization();

                app.MapControllers();

                Log.Information("Tippr API started successfully");

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
        }
    }
}
