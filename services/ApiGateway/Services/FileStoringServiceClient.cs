using System;
using System.Net.Http;
using System.Threading.Tasks;
using AntiPlagiarismSystem.Shared.Interfaces;
using AntiPlagiarismSystem.Shared.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using AntiPlagiarismSystem.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Services
{
    public class FileStoringServiceClient : IFileStoringService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FileStoringServiceClient> _logger;

        public FileStoringServiceClient(HttpClient httpClient, ILogger<FileStoringServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Guid> StoreFileAsync(byte[] fileContent, string fileName)
        {
            try
            {
                using var formData = new MultipartFormDataContent();
                using var fileContentStream = new MemoryStream(fileContent);
                formData.Add(new StreamContent(fileContentStream), "file", fileName);

                var response = await _httpClient.PostAsync("api/files", formData);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Guid>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error communicating with File Storing Service");
                throw new ServiceUnavailableException("File Storing Service is currently unavailable", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during file upload");
                throw;
            }
        }

        public async Task<byte[]> GetFileAsync(Guid fileId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/files/{fileId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error communicating with File Storing Service");
                throw new ServiceUnavailableException("File Storing Service is currently unavailable", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting file");
                throw;
            }
        }

        public async Task<FileMetadata> GetFileMetadataAsync(Guid fileId)
        {
            var response = await _httpClient.GetAsync($"api/files/{fileId}/metadata");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<FileMetadata>();
        }

        public async Task<Guid> StoreFileAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return await StoreFileAsync(memoryStream.ToArray(), file.FileName);
        }
    }
} 