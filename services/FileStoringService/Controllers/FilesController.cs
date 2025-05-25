using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AntiPlagiarismSystem.Shared.Interfaces;
using AntiPlagiarismSystem.Shared.Models;

namespace FileStoringService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileStoringService _fileStoringService;

        public FilesController(IFileStoringService fileStoringService)
        {
            _fileStoringService = fileStoringService;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var fileId = await _fileStoringService.StoreFileAsync(file);
            return Ok(fileId);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<byte[]>> GetFile(Guid id)
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

        [HttpGet("{id}/metadata")]
        public async Task<ActionResult<FileMetadata>> GetFileMetadata(Guid id)
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
    }
} 