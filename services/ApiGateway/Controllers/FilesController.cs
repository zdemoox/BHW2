using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AntiPlagiarismSystem.Shared.Interfaces;
using AntiPlagiarismSystem.Shared.Models;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FilesController : ControllerBase
    {
        private readonly IFileStoringService _fileStoringService;
        private readonly IFileAnalysisService _fileAnalysisService;

        public FilesController(IFileStoringService fileStoringService, IFileAnalysisService fileAnalysisService)
        {
            _fileStoringService = fileStoringService;
            _fileAnalysisService = fileAnalysisService;
        }

        /// <summary>
        /// Uploads a file for analysis
        /// </summary>
        /// <param name="file">The file to upload (must be .txt)</param>
        /// <returns>The ID of the uploaded file</returns>
        /// <response code="200">Returns the file ID</response>
        /// <response code="400">If the file is null or not a .txt file</response>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            if (!file.FileName.EndsWith(".txt"))
                return BadRequest("Only .txt files are supported");

            var fileId = await _fileStoringService.StoreFileAsync(file);
            return Ok(fileId);
        }

        /// <summary>
        /// Gets a file by its ID
        /// </summary>
        /// <param name="id">The ID of the file to retrieve</param>
        /// <returns>The file content</returns>
        /// <response code="200">Returns the file content</response>
        /// <response code="404">If the file is not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFile(Guid id)
        {
            try
            {
                var fileContent = await _fileStoringService.GetFileAsync(id);
                return File(fileContent, "text/plain");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Gets file metadata by ID
        /// </summary>
        /// <param name="id">The ID of the file</param>
        /// <returns>The file metadata</returns>
        /// <response code="200">Returns the file metadata</response>
        /// <response code="404">If the file is not found</response>
        [HttpGet("{id}/metadata")]
        [ProducesResponseType(typeof(FileMetadata), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFileMetadata(Guid id)
        {
            try
            {
                var metadata = await _fileStoringService.GetFileMetadataAsync(id);
                return Ok(metadata);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Analyzes a file by its ID
        /// </summary>
        /// <param name="id">The ID of the file to analyze</param>
        /// <returns>The analysis results</returns>
        /// <response code="200">Returns the analysis results</response>
        /// <response code="404">If the file is not found</response>
        [HttpPost("{id}/analyze")]
        [ProducesResponseType(typeof(AnalysisResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AnalyzeFile(Guid id)
        {
            try
            {
                var result = await _fileAnalysisService.AnalyzeFileAsync(id);
                return Ok(result);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Gets a word cloud for a file
        /// </summary>
        /// <param name="id">The ID of the file</param>
        /// <returns>The word cloud image</returns>
        /// <response code="200">Returns the word cloud image</response>
        /// <response code="404">If the file or word cloud is not found</response>
        [HttpGet("{id}/wordcloud")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWordCloud(Guid id)
        {
            try
            {
                var wordCloudImage = await _fileAnalysisService.GetWordCloudAsync(id);
                return File(wordCloudImage, "image/png");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
} 