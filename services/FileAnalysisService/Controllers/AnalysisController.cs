using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AntiPlagiarismSystem.Shared.Interfaces;
using AntiPlagiarismSystem.Shared.Models;

namespace FileAnalysisService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IFileAnalysisService _analysisService;
        private readonly IFileStoringService _fileStoringService;

        public AnalysisController(
            IFileAnalysisService analysisService,
            IFileStoringService fileStoringService)
        {
            _analysisService = analysisService;
            _fileStoringService = fileStoringService;
        }

        [HttpPost("{fileId}")]
        public async Task<ActionResult<AnalysisResult>> AnalyzeFile(Guid fileId)
        {
            try
            {
                var result = await _analysisService.AnalyzeFileAsync(fileId);
                return Ok(result);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpGet("{fileId}/wordcloud")]
        public async Task<ActionResult<byte[]>> GetWordCloud(Guid fileId)
        {
            try
            {
                var wordCloud = await _analysisService.GetWordCloudAsync(fileId);
                return File(wordCloud, "image/png");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
} 