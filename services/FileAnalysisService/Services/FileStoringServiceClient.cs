using System;
using System.Net.Http;
using System.Threading.Tasks;
using AntiPlagiarismSystem.Shared.Interfaces;
using AntiPlagiarismSystem.Shared.Models;

namespace FileAnalysisService.Services
{
    public class FileStoringServiceClient : IFileStoringService
    {
        private readonly HttpClient _httpClient;

        public FileStoringServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Guid> StoreFileAsync(byte[] fileContent, string fileName)
        {
            using var formData = new MultipartFormDataContent();
            using var fileContentStream = new MemoryStream(fileContent);
            formData.Add(new StreamContent(fileContentStream), "file", fileName);

            var response = await _httpClient.PostAsync("api/files", formData);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Guid>();
        }

        public async Task<byte[]> GetFileAsync(Guid fileId)
        {
            var response = await _httpClient.GetAsync($"api/files/{fileId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<FileMetadata> GetFileMetadataAsync(Guid fileId)
        {
            var response = await _httpClient.GetAsync($"api/files/{fileId}/metadata");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<FileMetadata>();
        }

        public Task<Guid> StoreFileAsync(Microsoft.AspNetCore.Http.IFormFile file)
        {
            throw new NotImplementedException("StoreFileAsync(IFormFile) not implemented in FileStoringServiceClient");
        }
    }
} 