using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PolicyService.Application.Interfaces;
using PolicyService.Infrastructure.Services;
using Shared.Middleware;
using Serilog;
using PolicyService.Domain;
using PolicyService.Infrastructure.Data;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SmartSure - PolicyService API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("Logs/policy-service-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Swagger with JWT Authorize button
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartSure - PolicyService API", Version = "v1" });
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

    builder.Services.AddDbContext<PolicyDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("PolicyDb")));

    builder.Services.AddScoped<IPolicyManagementService, PolicyManagementService>();

    var app = builder.Build();

    // Initialize Database and Seed Default Policy Plans
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PolicyDbContext>();
        dbContext.Database.Migrate();
        
        if (!dbContext.PolicyPlans.Any())
        {
            dbContext.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('PolicyPlans', RESEED, 100);");
            dbContext.PolicyPlans.AddRange(new PolicyPlan[]
            {
                new PolicyPlan { Title = "Comprehensive Health Plan", Description = "Full medical, hospitalization, and emergency coverage across 5000+ network hospitals in India.", Type = PolicyType.Health, BasePremium = 2500.00m, CoverageLimit = 500000.00m, DurationMonths = 12, IsActive = true },
                new PolicyPlan { Title = "Family Health Care Plus", Description = "Affordable family medical coverage including outpatient care, maternity cover, and diagnostic checkups.", Type = PolicyType.Health, BasePremium = 4500.00m, CoverageLimit = 800000.00m, DurationMonths = 12, IsActive = true },
                new PolicyPlan { Title = "Senior Citizen Care Shield", Description = "Dedicated medical cover for seniors over 60, including pre-existing illnesses, daily hospital cash, and geriatric support.", Type = PolicyType.Health, BasePremium = 6000.00m, CoverageLimit = 600000.00m, DurationMonths = 12, IsActive = true },
                new PolicyPlan { Title = "Critical Illness Premium Cover", Description = "High-limit coverage targeting key critical illnesses including cardiac, oncology, renal care, and advanced surgeries.", Type = PolicyType.Health, BasePremium = 3500.00m, CoverageLimit = 1500000.00m, DurationMonths = 12, IsActive = true },
                new PolicyPlan { Title = "Executive Auto Shield", Description = "Comprehensive motor vehicle collision, third-party liability, and theft protection.", Type = PolicyType.Auto, BasePremium = 1800.00m, CoverageLimit = 350000.00m, DurationMonths = 12, IsActive = true },
                new PolicyPlan { Title = "Homeowner Security Guard", Description = "Property damage, natural disaster, and burglary coverage.", Type = PolicyType.Home, BasePremium = 1200.00m, CoverageLimit = 1500000.00m, DurationMonths = 12, IsActive = true },
                new PolicyPlan { Title = "Life Protector Supreme", Description = "Term life policy with critical illness benefit and terminal cover.", Type = PolicyType.Life, BasePremium = 3000.00m, CoverageLimit = 2500000.00m, DurationMonths = 12, IsActive = true }
            });
            dbContext.SaveChanges();
        }
        
        if (!dbContext.UserPolicies.Any())
        {
            dbContext.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('UserPolicies', RESEED, 200);");
            var defaultPolicy = new UserPolicy
            {
                UserId = 1,
                PolicyPlanId = 101,
                PremiumAmount = 2500.00m,
                CoverageLimit = 500000.00m,
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(-10).AddMonths(12),
                Status = PolicyStatus.Active,
                HasPreExistingConditions = false,
                IsSmoker = false,
                HasRecentHospitalization = false
            };
            dbContext.UserPolicies.Add(defaultPolicy);
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
    Log.Fatal(ex, "PolicyService API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
