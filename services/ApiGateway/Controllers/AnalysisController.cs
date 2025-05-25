using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.IO;
using AntiPlagiarismSystem.Shared.Interfaces;
using AntiPlagiarismSystem.Shared.Models;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IFileAnalysisService _fileAnalysisService;

        public AnalysisController(IFileAnalysisService fileAnalysisService)
        {
            _fileAnalysisService = fileAnalysisService;
        }

        [HttpGet("{fileId}/wordcloud")]
        public async Task<IActionResult> GetWordCloud(Guid fileId)
        {
            try
            {
                var wordCloud = await _fileAnalysisService.GetWordCloudAsync(fileId);
                return File(wordCloud, "image/png");
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("compare")]
        public async Task<ActionResult<SimilarityResult>> CompareFiles([FromBody] CompareFilesRequest request)
        {
            try
            {
                var result = await _fileAnalysisService.CompareFilesAsync(request.OriginalFileId, request.ComparedFileId);
                return Ok(result);
            }
            catch (FileNotFoundException)
            {
                return NotFound("One or both files not found");
            }
        }
    }

    public class CompareFilesRequest
    {
        public Guid OriginalFileId { get; set; }
        public Guid ComparedFileId { get; set; }
    }
} 