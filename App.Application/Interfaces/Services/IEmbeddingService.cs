using App.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Application.Interfaces.Services
{
    public interface IEmbeddingService
    {
        public Task<List<float[]>> ComputeEmbeddingsBatchAsync(List<string> texts);
        public Task GenerateAndSaveEmbeddingsFileAsync(string filePath);
        public Task<List<SymptomEmbedding>> GetAllEmbeddingsAsync();
    }
}
