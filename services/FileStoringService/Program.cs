using Microsoft.EntityFrameworkCore;
using AntiPlagiarismSystem.Shared.Interfaces;
using AntiPlagiarismSystem.Shared.Models;
using FileStoringService.Services;
using FileStoringService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "File Storing Service API",
        Version = "v1",
        Description = "API for storing and retrieving files"
    });
});

// Configure PostgreSQL
builder.Services.AddDbContext<FileStoringDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IFileStoringService, FileStoringServiceImplementation>();

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "File Storing Service API V1");
        c.RoutePrefix = string.Empty; // This will serve the Swagger UI at the root URL
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
