using Microsoft.EntityFrameworkCore;
using AntiPlagiarismSystem.Shared.Models;

namespace FileAnalysisService.Data;

public class FileAnalysisDbContext : DbContext
{
    public FileAnalysisDbContext(DbContextOptions<FileAnalysisDbContext> options) : base(options)
    {
    }

    public DbSet<AnalysisResult> AnalysisResults { get; set; } = null!;
    public DbSet<SimilarityResult> SimilarityResults { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnalysisResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileId).IsRequired();
            entity.Property(e => e.ParagraphCount).IsRequired();
            entity.Property(e => e.WordCount).IsRequired();
            entity.Property(e => e.CharacterCount).IsRequired();
            entity.Property(e => e.WordCloudLocation).IsRequired(false);
        });

        modelBuilder.Entity<SimilarityResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OriginalFileId).IsRequired();
            entity.Property(e => e.ComparedFileId).IsRequired();
            entity.Property(e => e.SimilarityPercentage).IsRequired();
            entity.Property(e => e.ComparisonDate).IsRequired();
        });
    }
} 