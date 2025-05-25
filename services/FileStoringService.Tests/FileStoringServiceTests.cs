using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using AntiPlagiarismSystem.Shared.Models;
using FileStoringService.Services;
using FileStoringService.Data;

namespace FileStoringService.Tests
{
    public class FileStoringServiceTests : IDisposable
    {
        private readonly FileStoringDbContext _context;
        private readonly string _testStoragePath;
        private readonly Services.FileStoringService _service;

        public FileStoringServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<FileStoringDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new FileStoringDbContext(options);

            // Setup test storage path
            _testStoragePath = Path.Combine(Path.GetTempPath(), "FileStoringServiceTests");
            Directory.CreateDirectory(_testStoragePath);

            // Setup configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"FileStorage:BasePath", _testStoragePath}
                })
                .Build();

            _service = new Services.FileStoringService(_context, configuration);
        }

        [Fact]
        public async Task StoreFileAsync_NewFile_ReturnsNewId()
        {
            // Arrange
            var content = "Test content";
            var fileName = "test.txt";
            var formFile = CreateFormFile(content, fileName);

            // Act
            var fileId = await _service.StoreFileAsync(formFile);

            // Assert
            Assert.NotEqual(Guid.Empty, fileId);
            var storedFile = await _context.Files.FindAsync(fileId);
            Assert.NotNull(storedFile);
            Assert.Equal(fileName, storedFile.Name);
            Assert.True(File.Exists(storedFile.Location));
            Assert.Equal(content, await File.ReadAllTextAsync(storedFile.Location));
        }

        [Fact]
        public async Task StoreFileAsync_DuplicateFile_ReturnsSameId()
        {
            // Arrange
            var content = "Test content";
            var fileName1 = "test1.txt";
            var fileName2 = "test2.txt";
            var formFile1 = CreateFormFile(content, fileName1);
            var formFile2 = CreateFormFile(content, fileName2);

            // Act
            var fileId1 = await _service.StoreFileAsync(formFile1);
            var fileId2 = await _service.StoreFileAsync(formFile2);

            // Assert
            Assert.Equal(fileId1, fileId2);
            var files = await _context.Files.ToListAsync();
            Assert.Single(files);
        }

        [Fact]
        public async Task GetFileAsync_ExistingFile_ReturnsContent()
        {
            // Arrange
            var content = "Test content";
            var fileName = "test.txt";
            var formFile = CreateFormFile(content, fileName);
            var fileId = await _service.StoreFileAsync(formFile);

            // Act
            var retrievedContent = await _service.GetFileAsync(fileId);

            // Assert
            Assert.Equal(content, Encoding.UTF8.GetString(retrievedContent));
        }

        [Fact]
        public async Task GetFileAsync_NonExistingFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => 
                _service.GetFileAsync(nonExistingId));
        }

        [Fact]
        public async Task GetFileMetadataAsync_ExistingFile_ReturnsMetadata()
        {
            // Arrange
            var content = "Test content";
            var fileName = "test.txt";
            var formFile = CreateFormFile(content, fileName);
            var fileId = await _service.StoreFileAsync(formFile);

            // Act
            var metadata = await _service.GetFileMetadataAsync(fileId);

            // Assert
            Assert.NotNull(metadata);
            Assert.Equal(fileId, metadata.Id);
            Assert.Equal(fileName, metadata.Name);
            Assert.NotNull(metadata.Hash);
            Assert.NotNull(metadata.Location);
        }

        [Fact]
        public async Task GetFileMetadataAsync_NonExistingFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => 
                _service.GetFileMetadataAsync(nonExistingId));
        }

        private IFormFile CreateFormFile(string content, string fileName)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            var formFile = new Mock<IFormFile>();
            
            formFile.Setup(f => f.FileName).Returns(fileName);
            formFile.Setup(f => f.Length).Returns(bytes.Length);
            formFile.Setup(f => f.OpenReadStream()).Returns(stream);
            formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream stream, CancellationToken token) =>
                {
                    return stream.WriteAsync(bytes, 0, bytes.Length, token);
                });

            return formFile.Object;
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            
            if (Directory.Exists(_testStoragePath))
                Directory.Delete(_testStoragePath, true);
        }
    }
} 