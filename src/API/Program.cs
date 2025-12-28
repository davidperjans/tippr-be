using API.Auth;
using API.Hubs;
using API.Middleware;
using Application;
using Infrastructure;
using Infrastructure.Auth;
using Infrastructure.Data;
using Infrastructure.Data.Seeding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Serilog
// --------------------
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext();

    // Undvik ReadFrom.Services i Testing om inga sinks �r registrerade
    if (!context.HostingEnvironment.IsEnvironment("Testing"))
    {
        configuration.ReadFrom.Services(services);
    }
});

// --------------------
// Services
// --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

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
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tippr API",
        Version = "v1",
        Description = "Football prediction league API. Authenticate with Supabase JWT token."
    });

    // Include XML documentation
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste Supabase access_token"
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

// --------------------
// Authentication / Authorization
// --------------------
builder.Services
    .AddAuthentication("SupabaseAuth")
    .AddScheme<SupabaseAuthenticationOptions, SupabaseAuthenticationHandler>(
        "SupabaseAuth", _ => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",
        policy => policy.Requirements.Add(new AdminRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, AdminRequirementHandler>();

builder.Services.AddTransient<ErrorHandlingMiddleware>();

// --------------------
// Build app
// --------------------
var app = builder.Build();

// --------------------
// Middleware pipeline
// --------------------
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set(
                "UserId",
                httpContext.User.FindFirst("user_id")?.Value
            );
        }
    };
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<ChatHub>("/hubs/chat");

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseMiddleware<UserSyncMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Required for integration tests
public partial class Program { }
