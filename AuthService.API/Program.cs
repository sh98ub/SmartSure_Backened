using System.Text;
using AuthService.Application.Interfaces;
using AuthService.Infrasturcture.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Shared.Middleware;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using AuthService.Domain;
using AuthService.Infrasturcture.Data;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SmartSure - AuthService API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("Logs/auth-service-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartSure - AuthService API", Version = "v1" });
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

    builder.Services.AddDbContext<AuthDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDb")));

    builder.Services.AddSingleton<IJwtTokenGenerator, AuthService.API.Services.JwtTokenGenerator>();
    builder.Services.AddScoped<IUserRepository, AuthService.Infrasturcture.Repositories.UserRepository>();
    builder.Services.AddScoped<IUserService, UserService>();

    var app = builder.Build();

    // Initialize Database and Seed Default Users
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        dbContext.Database.Migrate();
        
        if (!dbContext.Users.Any())
        {
            dbContext.Users.AddRange(new User[]
            {
                new User
                {
                    Username = "admin",
                    Email = "admin@smartsure.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12),
                    FullName = "System Administrator",
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Username = "adjuster",
                    Email = "adjuster@smartsure.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Adjuster@123", workFactor: 12),
                    FullName = "Senior Claims Adjuster",
                    Role = UserRole.ClaimsAdjuster,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Username = "john_doe",
                    Email = "john@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123", workFactor: 12),
                    FullName = "John Doe",
                    Role = UserRole.PolicyHolder,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
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
    Log.Fatal(ex, "AuthService API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
