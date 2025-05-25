using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using AntiPlagiarismSystem.Shared.Interfaces;
using AntiPlagiarismSystem.Shared.Models;
using FileAnalysisService.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.IO;
using System.Threading;
using FileAnalysisService.Data;

namespace FileAnalysisService.Tests
{
    public class FileAnalysisServiceTests : IDisposable
    {
        private readonly FileAnalysisDbContext _context;
        private readonly Mock<IFileStoringService> _fileStoringServiceMock;
        private readonly FileAnalysisServiceImplementation _service;
        private readonly string _testFilePath;

        public FileAnalysisServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<FileAnalysisDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new FileAnalysisDbContext(options);

            // Setup file storage path
            _testFilePath = Path.Combine(Path.GetTempPath(), "FileAnalysisServiceTests");
            if (!Directory.Exists(_testFilePath))
            {
                Directory.CreateDirectory(_testFilePath);
            }

            // Setup mocks
            _fileStoringServiceMock = new Mock<IFileStoringService>();

            // Setup configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "FileStorage:Path", _testFilePath },
                    { "WordCloudApi:Url", "https://wordcloudapi.com/api/v1/wordcloud" }
                })
                .Build();

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());

            _service = new FileAnalysisServiceImplementation(
                _context,
                _fileStoringServiceMock.Object,
                httpClientFactory.Object,
                configuration
            );
        }

        [Fact]
        public async Task AnalyzeFileAsync_NewAnalysis_ReturnsResults()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var testText = "This is a test file.\n\nIt has multiple paragraphs.\n\nAnd some words.";
            var fileContent = Encoding.UTF8.GetBytes(testText);
            _fileStoringServiceMock.Setup(x => x.GetFileAsync(fileId))
                .ReturnsAsync(fileContent);

            // Act
            var result = await _service.AnalyzeFileAsync(fileId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fileId, result.FileId);
            Assert.Equal(3, result.ParagraphCount);
            Assert.Equal(12, result.WordCount);
            Assert.Equal(testText.Length, result.CharacterCount);
        }

        [Fact]
        public async Task AnalyzeFileAsync_ExistingAnalysis_ReturnsCachedResults()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var existingResult = new AnalysisResult
            {
                FileId = fileId,
                ParagraphCount = 3,
                WordCount = 15,
                CharacterCount = 100,
                WordCloudLocation = "path/to/wordcloud.png"
            };
            _context.AnalysisResults.Add(existingResult);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.AnalyzeFileAsync(fileId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingResult.ParagraphCount, result.ParagraphCount);
            Assert.Equal(existingResult.WordCount, result.WordCount);
            Assert.Equal(existingResult.CharacterCount, result.CharacterCount);
            Assert.Equal(existingResult.WordCloudLocation, result.WordCloudLocation);

            // Verify that file was not retrieved
            _fileStoringServiceMock.Verify(s => s.GetFileAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetWordCloudAsync_ExistingWordCloud_ReturnsImage()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var wordCloudPath = Path.Combine(_testFilePath, $"wordcloud_{fileId}.png");
            var wordCloudContent = Encoding.UTF8.GetBytes("fake image content");
            await File.WriteAllBytesAsync(wordCloudPath, wordCloudContent);

            var analysisResult = new AnalysisResult
            {
                Id = Guid.NewGuid(),
                FileId = fileId,
                WordCloudLocation = wordCloudPath
            };
            _context.AnalysisResults.Add(analysisResult);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetWordCloudAsync(fileId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(wordCloudContent, result);
        }

        [Fact]
        public async Task GetWordCloudAsync_NonExistingAnalysis_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => 
                _service.GetWordCloudAsync(nonExistingId));
        }

        [Fact]
        public async Task CompareFilesAsync_ReturnsSimilarityResult()
        {
            // Arrange
            var originalFileId = Guid.NewGuid();
            var comparedFileId = Guid.NewGuid();
            var originalContent = Encoding.UTF8.GetBytes("This is the original text.");
            var comparedContent = Encoding.UTF8.GetBytes("This is the compared text.");

            _fileStoringServiceMock.Setup(x => x.GetFileAsync(originalFileId))
                .ReturnsAsync(originalContent);
            _fileStoringServiceMock.Setup(x => x.GetFileAsync(comparedFileId))
                .ReturnsAsync(comparedContent);

            // Act
            var result = await _service.CompareFilesAsync(originalFileId, comparedFileId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(originalFileId, result.OriginalFileId);
            Assert.Equal(comparedFileId, result.ComparedFileId);
            Assert.True(result.SimilarityPercentage > 0);
            Assert.True(result.SimilarityPercentage <= 100);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            if (Directory.Exists(_testFilePath))
            {
                Directory.Delete(_testFilePath, true);
            }
        }
    }
} 