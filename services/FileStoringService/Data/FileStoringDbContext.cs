using Microsoft.EntityFrameworkCore;
using AntiPlagiarismSystem.Shared.Models;

namespace FileStoringService.Data;

public class FileStoringDbContext : DbContext
{
    public FileStoringDbContext(DbContextOptions<FileStoringDbContext> options) : base(options)
    {
    }

    public DbSet<FileMetadata> Files { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Hash).IsRequired();
            entity.Property(e => e.Location).IsRequired(false);
        });
    }
} 