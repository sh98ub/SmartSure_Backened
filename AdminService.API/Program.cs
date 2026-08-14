using System.Text;
using AdminService.Application.Interfaces;
using AdminService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Middleware;
using Serilog;
using AdminService.Domain;
using AdminService.Infrastructure.Data;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SmartSure - AdminService API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("Logs/admin-service-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Swagger with JWT Authorize button
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartSure - AdminService API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
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

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
    });

    // JWT Authentication
    var key = Encoding.ASCII.GetBytes(
        builder.Configuration["Jwt:SecretKey"]
        ?? throw new InvalidOperationException("Jwt:SecretKey not found in configuration."));
    builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

    builder.Services.AddDbContext<AdminDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("AdminDb")));

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddHttpClient();
    builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

    var app = builder.Build();

    // Initialize Database and Seed Default Data
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        dbContext.Database.EnsureCreated();
        
        if (!dbContext.AuditLogs.Any())
        {
            dbContext.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('AuditLogs', RESEED, 0);");
            dbContext.AuditLogs.AddRange(new AuditLog[]
            {
                new AuditLog
                {
                    Timestamp = DateTime.UtcNow.AddHours(-12),
                    Actor = "SystemAdmin",
                    Action = "System Initialization",
                    Details = "SmartSure Microservices stack initialized successfully.",
                    IpAddress = "127.0.0.1"
                },
                new AuditLog
                {
                    Timestamp = DateTime.UtcNow.AddHours(-4),
                    Actor = "Adjuster",
                    Action = "Claim Inspection",
                    Details = "Adjuster assigned to claim CLM-2026-0001.",
                    IpAddress = "192.168.1.45"
                }
            });
            dbContext.SaveChanges();
        }

        if (!dbContext.UserOverviews.Any())
        {
            dbContext.UserOverviews.AddRange(new AdminUserOverview[]
            {
                new AdminUserOverview { Username = "admin", Email = "admin@smartsure.com", FullName = "System Administrator", KycStatus = "Verified", Role = "Admin", CreatedAt = DateTime.UtcNow, IsActive = true },
                new AdminUserOverview { Username = "adjuster", Email = "adjuster@smartsure.com", FullName = "Senior Claims Adjuster", KycStatus = "Verified", Role = "ClaimsAdjuster", CreatedAt = DateTime.UtcNow, IsActive = true },
                new AdminUserOverview { Username = "john_doe", Email = "john@example.com", FullName = "John Doe", KycStatus = "Verified", Role = "PolicyHolder", CreatedAt = DateTime.UtcNow, IsActive = true }
            });
            dbContext.SaveChanges();
        }
    }

    app.UseSerilogRequestLogging();
    app.UseCorrelationId();
    app.UseSharedGlobalExceptionHandler();
    app.UseCors("AllowAll");
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AdminService API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
