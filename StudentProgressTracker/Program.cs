using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Serilog;
using StudentProgressTracker.Data;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services;
using StudentProgressTracker.Services.Interfaces;
using StudentProgressTracker.Configuration;
using StudentProgressTracker.HealthChecks;
using StudentProgressTracker.Middleware;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/studentprogresstracker-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                      "Data Source=StudentProgressTracker.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (connectionString.Contains("Data Source=") && connectionString.EndsWith(".db"))
    {
        // SQLite configuration
        options.UseSqlite(connectionString);
    }
    else
    {
        // SQL Server configuration
        options.UseSqlServer(connectionString);
    }
    
    // Enable sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Identity configuration
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("TeacherOrAdmin", policy => policy.RequireRole("Admin", "Teacher"));
    options.AddPolicy("StudentAccess", policy => policy.RequireRole("Admin", "Teacher", "Student"));
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Cache Configuration - Redis with In-Memory Fallback
var cacheOptions = builder.Configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>() ?? new CacheOptions();

// Always add memory cache for fallback
builder.Services.AddMemoryCache();

// Configure Redis if connection string is provided
if (!string.IsNullOrEmpty(cacheOptions.ConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = cacheOptions.ConnectionString;
        options.InstanceName = cacheOptions.InstanceName;
    });
    
    Log.Information("Redis cache configured at {ConnectionString}", cacheOptions.ConnectionString);
}
else
{
    Log.Information("No Redis connection string provided, using memory cache only");
}

// Use hybrid cache service that handles Redis fallback automatically
builder.Services.AddScoped<ICacheService, HybridCacheService>();
builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection(CacheOptions.SectionName));

// Rate Limiting Configuration
builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection(RateLimitOptions.SectionName));
builder.Services.AddSingleton<IRateLimitService, InMemoryRateLimitService>();

// Application Services
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IReportService, ReportService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? 
                           new[] { "http://localhost:3000", "http://localhost:5173" };
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Student Progress Tracker API",
        Version = "v1.0",
        Description = "A comprehensive REST API for tracking student progress and performance with advanced rate limiting",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@studenttracker.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // JWT Authentication setup for Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // Include XML comments for better documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddCheck<CacheHealthCheck>("cache");

// HTTP Client for external services
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline

// Global exception handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(error.Error, "Unhandled exception occurred");
            
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "An internal server error occurred",
                errors = app.Environment.IsDevelopment() ? new[] { error.Error.Message } : new[] { "Internal server error" }
            }));
        }
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Progress Tracker API v1.0");
        options.RoutePrefix = string.Empty; // Makes Swagger available at the app's root
        options.DocumentTitle = "Student Progress Tracker API";
        options.DefaultModelsExpandDepth(-1); // Hide schemas section by default
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        
        // Add rate limiting information to Swagger UI
        options.HeadContent = @"
            <style>
                .swagger-ui .info .title::after {
                    content: ' 🛡️';
                    font-size: 0.8em;
                }
                .swagger-ui .info .description::after {
                    content: '\A\A⚡ This API includes comprehensive rate limiting protection. See /api/v1.0/ratelimit/configuration for details.';
                    white-space: pre;
                    color: #3b82f6;
                    font-style: italic;
                }
            </style>";
    });
    
    // Enable detailed error pages in development
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-Rate-Limit-Enabled"] = "true";
    
    await next();
});

app.UseCors("AllowedOrigins");

// Add Rate Limiting Middleware (before authentication)
app.UseRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

// Request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "Handled {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null 
        ? Serilog.Events.LogEventLevel.Error 
        : Serilog.Events.LogEventLevel.Information;
    
    // Enrich with rate limiting information
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
        
        if (httpContext.Request.Headers.TryGetValue("X-Client-Id", out var clientId))
        {
            diagnosticContext.Set("ClientId", clientId.FirstOrDefault());
        }
        
        if (httpContext.Response.Headers.ContainsKey("X-RateLimit-Remaining-Minute"))
        {
            diagnosticContext.Set("RateLimitRemaining", httpContext.Response.Headers["X-RateLimit-Remaining-Minute"].FirstOrDefault());
        }
    };
});

app.MapControllers();
app.MapHealthChecks("/health");

// Database initialization and seeding
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var localLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        localLogger.LogInformation("Initializing database...");
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Run migrations if using SQL Server
        if (context.Database.GetDbConnection().GetType().Name.Contains("SqlConnection"))
        {
            await context.Database.MigrateAsync();
        }
        
        // Seed data
        await ApplicationDbContext.SeedDataAsync(context, userManager, roleManager, localLogger);
        
        localLogger.LogInformation("Database initialization completed successfully");
    }
    catch (Exception ex)
    {
        var loggerLocal = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        loggerLocal.LogError(ex, "An error occurred while initializing the database");
        throw;
    }
}

// Welcome message with rate limiting information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var rateLimitOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RateLimitOptions>>().Value;

logger.LogInformation("Student Progress Tracker API is starting...");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Rate Limiting: {Status}", rateLimitOptions.Enabled ? "Enabled" : "Disabled");
if (rateLimitOptions.Enabled)
{
    logger.LogInformation("Rate Limit - General Rules: {RequestsPerMinute}/min, {RequestsPerHour}/hour, {RequestsPerDay}/day", 
        rateLimitOptions.GeneralRules.RequestsPerMinute,
        rateLimitOptions.GeneralRules.RequestsPerHour,
        rateLimitOptions.GeneralRules.RequestsPerDay);
    logger.LogInformation("Rate Limit - Client Tiers: {TierCount} configured", rateLimitOptions.ClientTiers.Count);
    logger.LogInformation("Rate Limit - Endpoint Rules: {EndpointCount} configured", rateLimitOptions.EndpointRules.Count);
    logger.LogInformation("Rate Limit - Whitelisted IPs: {IPCount}", rateLimitOptions.WhitelistedIPs.Count);
}
logger.LogInformation("Swagger UI available at: {BaseUrl}", app.Environment.IsDevelopment() ? "https://localhost:7207" : "Application root");
logger.LogInformation("Rate Limit Management: {BaseUrl}/api/v1.0/ratelimit/configuration", app.Environment.IsDevelopment() ? "https://localhost:7207" : "Application root");

app.Run();
