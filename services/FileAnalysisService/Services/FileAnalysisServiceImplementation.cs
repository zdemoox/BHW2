using System.Text;
using AntiPlagiarismSystem.Shared.Interfaces;
using AntiPlagiarismSystem.Shared.Models;
using FileAnalysisService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FileAnalysisService.Models;

namespace FileAnalysisService.Services;

public class FileAnalysisServiceImplementation : IFileAnalysisService
{
    private readonly FileAnalysisDbContext _context;
    private readonly IFileStoringService _fileStoringService;
    private readonly HttpClient _httpClient;
    private readonly string _wordCloudApiUrl;
    private readonly string _fileStoragePath;

    public FileAnalysisServiceImplementation(
        FileAnalysisDbContext context,
        IFileStoringService fileStoringService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _context = context;
        _fileStoringService = fileStoringService;
        _httpClient = httpClientFactory.CreateClient();
        _wordCloudApiUrl = configuration.GetValue<string>("WordCloudApi:Url") ?? "https://wordcloudapi.com/api/v1/wordcloud";
        _fileStoragePath = configuration.GetValue<string>("FileStorage:Path") ?? "FileStorage";
    }

    public async Task<AnalysisResult> AnalyzeFileAsync(Guid fileId)
    {
        var existingResult = await _context.AnalysisResults
            .FirstOrDefaultAsync(r => r.FileId == fileId);

        if (existingResult != null)
        {
            return existingResult;
        }

        var fileContent = await _fileStoringService.GetFileAsync(fileId);
        var text = Encoding.UTF8.GetString(fileContent);

        var paragraphs = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        var words = text.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var characters = text.Length;

        var result = new AnalysisResult
        {
            Id = Guid.NewGuid(),
            FileId = fileId,
            ParagraphCount = paragraphs.Length,
            WordCount = words.Length,
            CharacterCount = characters
        };

        try
        {
            var wordCloudResponse = await _httpClient.PostAsJsonAsync(_wordCloudApiUrl, new { text });
            if (wordCloudResponse.IsSuccessStatusCode)
            {
                var wordCloudImage = await wordCloudResponse.Content.ReadAsByteArrayAsync();
                var wordCloudPath = Path.Combine(_fileStoragePath, $"wordcloud_{fileId}.png");
                await File.WriteAllBytesAsync(wordCloudPath, wordCloudImage);
                result.WordCloudLocation = wordCloudPath;
            }
        }
        catch (Exception ex)
        {
            // Log the error but continue with the analysis
            Console.WriteLine($"Error generating word cloud: {ex.Message}");
        }

        _context.AnalysisResults.Add(result);
        await _context.SaveChangesAsync();

        return result;
    }

    public async Task<byte[]> GetWordCloudAsync(Guid fileId)
    {
        var result = await _context.AnalysisResults
            .FirstOrDefaultAsync(r => r.FileId == fileId);

        if (result == null || string.IsNullOrEmpty(result.WordCloudLocation))
        {
            throw new FileNotFoundException($"Word cloud not found for file {fileId}");
        }

        return await File.ReadAllBytesAsync(result.WordCloudLocation);
    }

    public async Task<SimilarityResult> CompareFilesAsync(Guid originalFileId, Guid comparedFileId)
    {
        var originalContent = await _fileStoringService.GetFileAsync(originalFileId);
        var comparedContent = await _fileStoringService.GetFileAsync(comparedFileId);

        var originalText = Encoding.UTF8.GetString(originalContent);
        var comparedText = Encoding.UTF8.GetString(comparedContent);

        var distance = CalculateLevenshteinDistance(originalText, comparedText);
        var maxLength = Math.Max(originalText.Length, comparedText.Length);
        var similarityPercentage = maxLength == 0 ? 100 : (1 - (double)distance / maxLength) * 100;

        var result = new SimilarityResult
        {
            Id = Guid.NewGuid(),
            OriginalFileId = originalFileId,
            ComparedFileId = comparedFileId,
            SimilarityPercentage = similarityPercentage,
            ComparisonDate = DateTime.UtcNow
        };

        _context.SimilarityResults.Add(result);
        await _context.SaveChangesAsync();

        return result;
    }

    private static int CalculateLevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; i++)
            d[i, 0] = i;
        for (int j = 0; j <= m; j++)
            d[0, j] = j;

        for (int j = 1; j <= m; j++)
            for (int i = 1; i <= n; i++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }

        return d[n, m];
    }
} 