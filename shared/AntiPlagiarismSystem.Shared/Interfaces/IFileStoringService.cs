using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AntiPlagiarismSystem.Shared.Models;

namespace AntiPlagiarismSystem.Shared.Interfaces
{
    public interface IFileStoringService
    {
        Task<Guid> StoreFileAsync(IFormFile file);
        Task<byte[]> GetFileAsync(Guid id);
        Task<FileMetadata> GetFileMetadataAsync(Guid id);
    }
} 