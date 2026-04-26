using Maba.Application;
using Maba.Infrastructure;
using Maba.Infrastructure.Data;
using Maba.Api.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Configure host options for background services
builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

// Remove ASP.NET Core request body and multipart form size limits.
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = null;
});
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = null;
});

// Add Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Add Application
builder.Services.AddApplication();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJWTTokenGenerationThatShouldBeAtLeast32CharactersLong";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var validAudience = jwtSettings["Audience"] ?? "MabaClient";
    var validIssuer = jwtSettings["Issuer"] ?? "MabaApi";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = validIssuer,
        ValidAudience = validAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
    // SignalR: accept token from query string (WebSockets don't send Authorization header)
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var path = context.Request.Path;
            if (path.StartsWithSegments("/hubs"))
            {
                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token))
                    context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    Maba.Api.Authorization.AuthorizationPolicies.ConfigurePolicies(options);
});

// Register permission handler
builder.Services.AddScoped<IAuthorizationHandler, Maba.Api.Authorization.PermissionHandler>();
builder.Services.AddScoped<Maba.Api.Services.SupportChatMessagingService>();
builder.Services.AddScoped<Maba.Api.Services.AdminNotificationService>();
builder.Services.AddHttpContextAccessor();

// Rate limiting: protect API from abuse and DoS (built-in .NET 8)
var rateLimitWindowMinutes = builder.Configuration.GetValue("RateLimit:WindowMinutes", 1);
var rateLimitPermitLimit = builder.Configuration.GetValue("RateLimit:PermitLimit", 100);
var authPermitLimit = builder.Configuration.GetValue("RateLimit:AuthPermitLimit", 10);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, _) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString("F0");
        await context.HttpContext.Response.WriteAsJsonAsync(new { error = "Too many requests. Please try again later." });
    };
    // Global limit for all endpoints (per IP)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = rateLimitPermitLimit,
            Window = TimeSpan.FromMinutes(rateLimitWindowMinutes),
            QueueLimit = 0
        });
    });
    // Stricter named policy for auth endpoints (login/register) – applied via [EnableRateLimiting("auth")] on AuthController
    options.AddPolicy("auth", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = authPermitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
});

// Add CORS - accept requests from any frontend origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:4201",
                "http://localhost:3000",
                "http://127.0.0.1:4200",
                "https://mabasol.com",
                "https://app.mabasol.com"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});


var app = builder.Build();

// Apply EF migrations on startup only when explicitly enabled. In production this should
// normally be handled as a deployment step so a migration failure does not take down the API.
var applyMigrationsOnStartup = builder.Configuration.GetValue("Database:ApplyMigrationsOnStartup", app.Environment.IsDevelopment());
if (applyMigrationsOnStartup)
{
    using var migrationScope = app.Services.CreateScope();
    var migrationLogger = migrationScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        var pending = await db.Database.GetPendingMigrationsAsync();
        var pendingList = pending.ToList();
        if (pendingList.Count > 0)
        {
            migrationLogger.LogInformation(
                "Applying {Count} pending database migration(s): {Migrations}",
                pendingList.Count,
                string.Join(", ", pendingList));
        }

        await db.Database.MigrateAsync();
        migrationLogger.LogInformation("Database schema is up to date.");
    }
    catch (Exception ex)
    {
        migrationLogger.LogError(ex,
            "Failed to apply database migrations on startup. Fix the database or set Database:ApplyMigrationsOnStartup to false and run migrations manually.");
        throw;
    }
}

// Log email provider so it's clear whether verification/confirmation emails will be sent
var smtpHost = builder.Configuration["Smtp:Host"];
if (string.IsNullOrWhiteSpace(smtpHost))
    app.Logger.LogWarning("Email: Mock mode (Smtp:Host not set). Verification and request-confirmation emails will NOT be sent. Set Smtp in User Secrets or appsettings to send real emails.");
else
    app.Logger.LogInformation("Email: SMTP configured ({Host}). Verification and confirmation emails will be sent.", smtpHost);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandling();

// Only use HTTPS redirect in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
// Serve uploaded files from wwwroot (e.g. /uploads/Image/xxx.png)
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<Maba.Api.Hubs.SupportChatHub>("/hubs/support-chat");

// Check-email page (HTML) - used after registration so user is redirected here instead of Angular home
app.MapGet("/check-email", (IConfiguration config) =>
{
    var frontend = config["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
    var loginUrl = frontend + "/auth/login";
    var html = $@"<!DOCTYPE html><html lang=""en""><head><meta charset=""UTF-8""/><meta name=""viewport"" content=""width=device-width,initial-scale=1""/><title>Check your email - MABA</title><style>*{{box-sizing:border-box}}body{{font-family:system-ui,sans-serif;margin:0;min-height:100vh;display:flex;align-items:center;justify-content:center;background:linear-gradient(135deg,#0c1445 0%,#1a1a2e 50%,#16213e 100%);color:#fff;padding:1.5rem}}.card{{background:rgba(255,255,255,.95);color:#1a1a2e;border-radius:24px;padding:2.5rem;max-width:440px;width:100%;box-shadow:0 25px 50px rgba(0,0,0,.3)}}.icon{{width:72px;height:72px;margin:0 auto 1.5rem;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:2rem;background:linear-gradient(135deg,rgba(102,126,234,.2),rgba(118,75,162,.2));color:#667eea}}h1{{font-size:1.5rem;margin:0 0 .75rem;text-align:center}}.intro{{color:#6c757d;text-align:center;margin:0 0 1.5rem;line-height:1.5}}.steps{{list-style:none;padding:0;margin:0 0 1.5rem;counter-reset:step}}.steps li{{position:relative;padding-left:2.5rem;margin-bottom:1rem;color:#374151;line-height:1.5;counter-increment:step}}.steps li::before{{content:counter(step);position:absolute;left:0;width:1.75rem;height:1.75rem;border-radius:50%;background:linear-gradient(135deg,#667eea,#764ba2);color:#fff;font-size:.875rem;font-weight:700;line-height:1.75rem;text-align:center}}.steps li strong{{color:#1a1a2e}}.cta{{text-align:center;margin-top:1.5rem}}a.btn{{display:inline-flex;align-items:center;gap:.5rem;padding:.875rem 1.75rem;background:linear-gradient(135deg,#667eea,#764ba2);color:#fff;text-decoration:none;border-radius:12px;font-weight:600;font-size:1rem}}a.btn:hover{{opacity:.95;box-shadow:0 4px 20px rgba(102,126,234,.4)}}</style></head><body><div class=""card""><div class=""icon"">✉</div><h1>Check your email</h1><p class=""intro"">We've sent you an email to verify your account. Follow the steps below to get started.</p><ol class=""steps""><li><strong>Check your inbox</strong> (and spam folder) for an email from MABA.</li><li><strong>Click the verification link</strong> in the email. The link expires in 24 hours.</li><li><strong>Sign in</strong> after your email is verified — you can then use your account.</li></ol><p class=""intro"" style=""margin-bottom:0;font-size:.9rem"">Didn't receive the email? You can request a new link from the sign-in page.</p><div class=""cta""><a href=""{loginUrl}"" class=""btn"">Go to Sign in</a></div></div></body></html>";
    return Results.Content(html, "text/html");
});

// Apply migrations and seed database on startup (in development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<Maba.Infrastructure.Data.ApplicationDbContext>();
    
    try
    {
        logger.LogInformation("Checking database connection and migrations...");
        
        // Check if database exists
        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogInformation("Database does not exist. Creating database and applying migrations...");
        }
        
        // Get pending migrations
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        var pendingList = pendingMigrations.ToList();
        
        if (pendingList.Count > 0)
        {
            logger.LogInformation("Applying {Count} pending migration(s): {Migrations}", 
                pendingList.Count, string.Join(", ", pendingList));
        }
        
        // Apply all pending migrations (this also creates the database if it doesn't exist)
        await context.Database.MigrateAsync();
        logger.LogInformation("Database is up to date!");
        
        // Seed the database
        logger.LogInformation("Seeding database...");
        await scope.ServiceProvider.SeedDatabaseAsync();
        logger.LogInformation("Database seeding completed!");
    }
    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2714) // Object already exists
    {
        logger.LogWarning("Database has existing tables that conflict with migrations. " +
            "This usually happens when the database was created without EF Core migrations. " +
            "Attempting to recreate database...");
        
        try
        {
            // Delete and recreate the database
            await context.Database.EnsureDeletedAsync();
            logger.LogInformation("Old database deleted.");
            
            await context.Database.MigrateAsync();
            logger.LogInformation("Database recreated with migrations!");
            
            // Seed the database
            logger.LogInformation("Seeding database...");
            await scope.ServiceProvider.SeedDatabaseAsync();
            logger.LogInformation("Database seeding completed!");
        }
        catch (Exception innerEx)
        {
            logger.LogError(innerEx, "Failed to recreate database.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database. " +
            "Please ensure SQL Server is running and accessible.");
    }
}

app.Run();
