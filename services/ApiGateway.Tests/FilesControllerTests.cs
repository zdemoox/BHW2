using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using AntiPlagiarismSystem.Shared.Interfaces;
using AntiPlagiarismSystem.Shared.Models;
using ApiGateway.Controllers;

namespace ApiGateway.Tests
{
    public class FilesControllerTests
    {
        private readonly Mock<IFileStoringService> _mockFileStoringService;
        private readonly Mock<IFileAnalysisService> _mockFileAnalysisService;
        private readonly FilesController _controller;

        public FilesControllerTests()
        {
            _mockFileStoringService = new Mock<IFileStoringService>();
            _mockFileAnalysisService = new Mock<IFileAnalysisService>();
            _controller = new FilesController(_mockFileStoringService.Object, _mockFileAnalysisService.Object);
        }

        [Fact]
        public async Task UploadFile_ValidFile_ReturnsOkWithFileId()
        {
            // Arrange
            var content = "Test content";
            var fileName = "test.txt";
            var formFile = CreateFormFile(content, fileName);
            var expectedFileId = Guid.NewGuid();

            _mockFileStoringService.Setup(s => s.StoreFileAsync(formFile))
                .ReturnsAsync(expectedFileId);

            // Act
            var result = await _controller.UploadFile(formFile);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var fileId = Assert.IsType<Guid>(okResult.Value);
            Assert.Equal(expectedFileId, fileId);
        }

        [Fact]
        public async Task UploadFile_NullFile_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.UploadFile(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadFile_NonTxtFile_ReturnsBadRequest()
        {
            // Arrange
            var formFile = CreateFormFile("content", "test.pdf");

            // Act
            var result = await _controller.UploadFile(formFile);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetFile_ExistingFile_ReturnsFileContent()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var content = "Test content";
            var contentBytes = Encoding.UTF8.GetBytes(content);

            _mockFileStoringService.Setup(s => s.GetFileAsync(fileId))
                .ReturnsAsync(contentBytes);

            // Act
            var result = await _controller.GetFile(fileId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/plain", fileResult.ContentType);
            Assert.Equal(contentBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task GetFile_NonExistingFile_ReturnsNotFound()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            _mockFileStoringService.Setup(s => s.GetFileAsync(fileId))
                .ThrowsAsync(new FileNotFoundException());

            // Act
            var result = await _controller.GetFile(fileId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetFileMetadata_ExistingFile_ReturnsMetadata()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var metadata = new FileMetadata
            {
                Id = fileId,
                Name = "test.txt",
                Hash = "hash",
                Location = "path/to/file"
            };

            _mockFileStoringService.Setup(s => s.GetFileMetadataAsync(fileId))
                .ReturnsAsync(metadata);

            // Act
            var result = await _controller.GetFileMetadata(fileId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMetadata = Assert.IsType<FileMetadata>(okResult.Value);
            Assert.Equal(metadata.Id, returnedMetadata.Id);
            Assert.Equal(metadata.Name, returnedMetadata.Name);
            Assert.Equal(metadata.Hash, returnedMetadata.Hash);
            Assert.Equal(metadata.Location, returnedMetadata.Location);
        }

        [Fact]
        public async Task AnalyzeFile_ExistingFile_ReturnsAnalysisResults()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var analysisResult = new AnalysisResult
            {
                FileId = fileId,
                ParagraphCount = 2,
                WordCount = 10,
                CharacterCount = 50
            };

            _mockFileAnalysisService.Setup(s => s.AnalyzeFileAsync(fileId))
                .ReturnsAsync(analysisResult);

            // Act
            var result = await _controller.AnalyzeFile(fileId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResult = Assert.IsType<AnalysisResult>(okResult.Value);
            Assert.Equal(analysisResult.FileId, returnedResult.FileId);
            Assert.Equal(analysisResult.ParagraphCount, returnedResult.ParagraphCount);
            Assert.Equal(analysisResult.WordCount, returnedResult.WordCount);
            Assert.Equal(analysisResult.CharacterCount, returnedResult.CharacterCount);
        }

        [Fact]
        public async Task GetWordCloud_ExistingFile_ReturnsImage()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var imageBytes = new byte[] { 1, 2, 3, 4, 5 };

            _mockFileAnalysisService.Setup(s => s.GetWordCloudAsync(fileId))
                .ReturnsAsync(imageBytes);

            // Act
            var result = await _controller.GetWordCloud(fileId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("image/png", fileResult.ContentType);
            Assert.Equal(imageBytes, fileResult.FileContents);
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
    }
} 