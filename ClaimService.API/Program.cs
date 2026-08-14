using System.Text;
using ClaimService.Application.Interfaces;
using ClaimService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Middleware;
using Serilog;
using ClaimService.Domain;
using ClaimService.Infrastructure.Data;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SmartSure - ClaimService API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("Logs/claim-service-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Swagger with JWT Authorize button
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartSure - ClaimService API", Version = "v1" });
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

    builder.Services.AddDbContext<ClaimDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("ClaimDb")));
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddHttpClient();
    builder.Services.AddScoped<IClaimProcessingService, ClaimProcessingService>();

    var app = builder.Build();

    // Initialize Database and Seed Default Claims
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ClaimDbContext>();
        dbContext.Database.EnsureCreated();
        
        if (!dbContext.Claims.Any())
        {
            dbContext.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Claims', RESEED, 300);");
            var sampleClaim = new Claim
            {
                ClaimNumber = "CLM-2026-00301",
                UserPolicyId = 201,
                UserId = 1,
                IncidentDate = DateTime.UtcNow.AddDays(-10),
                ClaimAmount = 4500.00m,
                Description = "Vehicle bumper collision damage repair.",
                SupportingDocumentUrl = "https://example.com/docs/incident-report-001.pdf",
                Status = ClaimStatus.UnderReview,
                SubmittedAt = DateTime.UtcNow.AddDays(-9),
                Remarks = "Awaiting adjuster assessment."
            };
            dbContext.Claims.Add(sampleClaim);
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
    Log.Fatal(ex, "ClaimService API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
