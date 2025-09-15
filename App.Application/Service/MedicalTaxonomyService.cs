using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Application.Service
{
    public class MedicalTaxonomyService : IMedicalTaxonomyService
    {
        private readonly ISymptomEmbeddingRepository _embeddingRepository;
        private readonly IEmbeddingService _embeddingService;

        public MedicalTaxonomyService(ISymptomEmbeddingRepository embeddingRepository, IEmbeddingService embeddingService)
        {
            _embeddingRepository = embeddingRepository;
            _embeddingService = embeddingService;
        }

        public async Task<string> MapSymptomsToSpecialistAsync(List<string> extractedSymptoms)
        {
            var inputEmbedding =  await ComputeEmbedding(string.Join(" ", extractedSymptoms));
            var embeddings = await _embeddingRepository.GetAllEmbeddingsAsync();

            string bestMatchSpecialist = "General Practitioner";
            double highestSimilarity = 0;

            foreach (var record in embeddings)
            {
                var similarity = CosineSimilarity(inputEmbedding, record.EmbeddingVector);

                if (similarity > highestSimilarity)
                {
                    highestSimilarity = similarity;
                    bestMatchSpecialist = record.Specialist;
                }
            }

            return bestMatchSpecialist;
        }

        private async Task<float[]>ComputeEmbedding(string text)
        {
            var list = new List<string> { text };
            //return await _embeddingService.ComputeEmbeddingsBatchAsync(list);
            return (await _embeddingService.ComputeEmbeddingsBatchAsync(list)).First();
        }

        private static double CosineSimilarity(float[] vectorA, float[] vectorB)
        {
            double dot = 0, magA = 0, magB = 0;
            for (int i = 0; i < vectorA.Length; i++)
            {
                dot += vectorA[i] * vectorB[i];
                magA += vectorA[i] * vectorA[i];
                magB += vectorB[i] * vectorB[i];
            }
            return dot / (Math.Sqrt(magA) * Math.Sqrt(magB) + 1e-10);
        }
    }
}