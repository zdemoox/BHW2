using System;

namespace AntiPlagiarismSystem.Shared.Models
{
    public class FileMetadata
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Hash { get; set; }
        public string? Location { get; set; }
    }
} 