using Microsoft.EntityFrameworkCore;
using AntiPlagiarismSystem.Shared.Interfaces;
using AntiPlagiarismSystem.Shared.Models;
using FileAnalysisService.Services;
using FileAnalysisService.Data;
using FileAnalysisService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "File Analysis Service API",
        Version = "v1",
        Description = "API for analyzing text files and generating word clouds"
    });
});

// Configure PostgreSQL
builder.Services.AddDbContext<FileAnalysisDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure HttpClient
builder.Services.AddHttpClient();

// Register services
builder.Services.AddScoped<IFileAnalysisService, FileAnalysisServiceImplementation>();
builder.Services.AddScoped<IFileStoringService, FileStoringServiceClient>();

// Configure service URLs
builder.Services.Configure<ServiceUrls>(builder.Configuration.GetSection("ServiceUrls"));

// Configure file storage path
var fileStoragePath = builder.Configuration.GetValue<string>("FileStorage:Path") ?? "FileStorage";
if (!Directory.Exists(fileStoragePath))
{
    Directory.CreateDirectory(fileStoragePath);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "File Analysis Service API V1");
        c.RoutePrefix = string.Empty; // This will serve the Swagger UI at the root URL
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
