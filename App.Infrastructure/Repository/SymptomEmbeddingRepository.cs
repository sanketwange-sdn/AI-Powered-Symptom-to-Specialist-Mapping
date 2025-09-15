using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Domain.Entities;

namespace App.Infrastructure.Repository
{
    public class SymptomEmbeddingRepository : ISymptomEmbeddingRepository
    {

        private readonly IEmbeddingService _embeddingService;

        public SymptomEmbeddingRepository(IEmbeddingService embeddingService)
        {
            _embeddingService = embeddingService;
        }

        /// <summary>
        /// Call this method once to initialize the embeddings from the JSON file.
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            // Get the wwwroot path
            var currentDir = Directory.GetCurrentDirectory();
            var wwwrootPath = Path.Combine(currentDir, "wwwroot");
            // Combine path to symptom_embeddings.json
            var path = Path.Combine(wwwrootPath, "symptom_embeddings.json");
            await _embeddingService.GenerateAndSaveEmbeddingsFileAsync(path);
        }

        public async Task<List<SymptomEmbedding>> GetAllEmbeddingsAsync()
        {
            return await _embeddingService.GetAllEmbeddingsAsync();
        }
    }
}
