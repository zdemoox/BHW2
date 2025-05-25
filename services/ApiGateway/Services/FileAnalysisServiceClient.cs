using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
using AntiPlagiarismSystem.Shared.Interfaces;
using AntiPlagiarismSystem.Shared.Models;
using AntiPlagiarismSystem.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Services
{
    public class FileAnalysisServiceClient : IFileAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FileAnalysisServiceClient> _logger;

        public FileAnalysisServiceClient(HttpClient httpClient, ILogger<FileAnalysisServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<AnalysisResult> AnalyzeFileAsync(Guid fileId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/api/analysis/{fileId}", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<AnalysisResult>() 
                    ?? throw new InvalidOperationException("Failed to deserialize analysis result");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error communicating with File Analysis Service");
                throw new ServiceUnavailableException("File Analysis Service is currently unavailable", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during file analysis");
                throw;
            }
        }

        public async Task<byte[]> GetWordCloudAsync(Guid fileId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/analysis/{fileId}/wordcloud");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error communicating with File Analysis Service");
                throw new ServiceUnavailableException("File Analysis Service is currently unavailable", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting word cloud");
                throw;
            }
        }

        public async Task<SimilarityResult> CompareFilesAsync(Guid originalFileId, Guid comparedFileId)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/analysis/compare", 
                    new { OriginalFileId = originalFileId, ComparedFileId = comparedFileId });
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<SimilarityResult>() 
                    ?? throw new InvalidOperationException("Failed to deserialize similarity result");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error communicating with File Analysis Service");
                throw new ServiceUnavailableException("File Analysis Service is currently unavailable", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during file comparison");
                throw;
            }
        }
    }
} 