using System;

namespace AntiPlagiarismSystem.Shared.Models
{
    public class SimilarityResult
    {
        public Guid Id { get; set; }
        public Guid OriginalFileId { get; set; }
        public Guid ComparedFileId { get; set; }
        public double SimilarityPercentage { get; set; }
        public DateTime ComparisonDate { get; set; }
    }
} 