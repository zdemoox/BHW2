using System;
using System.Threading.Tasks;
using AntiPlagiarismSystem.Shared.Models;

namespace AntiPlagiarismSystem.Shared.Interfaces
{
    public interface IFileAnalysisService
    {
        Task<AnalysisResult> AnalyzeFileAsync(Guid fileId);
        Task<byte[]> GetWordCloudAsync(Guid fileId);
        Task<SimilarityResult> CompareFilesAsync(Guid originalFileId, Guid comparedFileId);
    }
} 