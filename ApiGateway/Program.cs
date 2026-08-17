
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Shared.Middleware;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add Services
builder.Services.AddOcelot(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Shared Middleware: Correlation ID & Global Exception Handler
app.UseCorrelationId();
app.UseSharedGlobalExceptionHandler();

app.UseCors("AllowAll");

// Unified Swagger Aggregation UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/auth/v1/swagger.json", "AuthService API (v1)");
    c.SwaggerEndpoint("/swagger/policy/v1/swagger.json", "PolicyService API (v1)");
    c.SwaggerEndpoint("/swagger/claim/v1/swagger.json", "ClaimService API (v1)");
    c.SwaggerEndpoint("/swagger/admin/v1/swagger.json", "AdminService API (v1)");
    c.RoutePrefix = "swagger";
    c.DocExpansion(DocExpansion.None);
});

// Redirect root URL to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

// Ocelot Gateway Middleware
await app.UseOcelot();

app.Run();
