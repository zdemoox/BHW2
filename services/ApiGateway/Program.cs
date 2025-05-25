using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using AntiPlagiarismSystem.Shared.Interfaces;
using AntiPlagiarismSystem.Shared.Models;
using ApiGateway.Services;
using ApiGateway.Data;
using ApiGateway.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Anti-Plagiarism System API",
        Version = "v1",
        Description = "API Gateway for the Anti-Plagiarism System"
    });
});

// Configure PostgreSQL
builder.Services.AddDbContext<ApiGatewayDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure HTTP clients
builder.Services.AddHttpClient<IFileStoringService, FileStoringServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["FileStoringService:BaseUrl"]);
});

builder.Services.AddHttpClient<IFileAnalysisService, FileAnalysisServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["FileAnalysisService:BaseUrl"]);
});

// Register services
builder.Services.AddScoped<IFileStoringService, FileStoringServiceClient>();
builder.Services.AddScoped<IFileAnalysisService, FileAnalysisServiceClient>();

// Configure service URLs
builder.Services.Configure<ServiceUrls>(builder.Configuration.GetSection("ServiceUrls"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Anti-Plagiarism System API V1");
        c.RoutePrefix = string.Empty; // This will serve the Swagger UI at the root URL
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
