using Microsoft.EntityFrameworkCore;
using AntiPlagiarismSystem.Shared.Models;

namespace ApiGateway.Data;

public class ApiGatewayDbContext : DbContext
{
    public ApiGatewayDbContext(DbContextOptions<ApiGatewayDbContext> options) : base(options)
    {
    }

    public DbSet<FileMetadata> Files { get; set; } = null!;
    public DbSet<AnalysisResult> AnalysisResults { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Hash).IsRequired();
            entity.Property(e => e.Location).IsRequired(false);
        });

        modelBuilder.Entity<AnalysisResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileId).IsRequired();
            entity.Property(e => e.ParagraphCount).IsRequired();
            entity.Property(e => e.WordCount).IsRequired();
            entity.Property(e => e.CharacterCount).IsRequired();
            entity.Property(e => e.WordCloudLocation).IsRequired(false);
        });
    }
} 