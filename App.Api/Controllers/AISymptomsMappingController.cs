using App.Application.Dto;
using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Application.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AISymptomsMappingController : BaseController
    {
        private readonly ISymptomAnalysisService _symptomAnalysisService;
        private readonly ISymptomEmbeddingRepository _symptomEmbedding;

        public AISymptomsMappingController(ISymptomAnalysisService symptomAnalysisService, ISymptomEmbeddingRepository symptomEmbedding)
        {
            _symptomAnalysisService = symptomAnalysisService;
            _symptomEmbedding = symptomEmbedding;
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeSymptomsAsync(string symptoms)
        {
            return Ok(await _symptomAnalysisService.AnalyzeSymptomsAsync(symptoms));
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> GenerateAndSaveEmbeddingsFile(string symptoms)
        {
                await _symptomEmbedding.InitializeAsync();
            return Ok();
        }
    }
}
