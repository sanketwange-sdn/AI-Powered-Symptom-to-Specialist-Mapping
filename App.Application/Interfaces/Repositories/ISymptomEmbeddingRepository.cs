using App.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Application.Interfaces.Repositories
{
    public interface ISymptomEmbeddingRepository 
    {
        Task<List<SymptomEmbedding>> GetAllEmbeddingsAsync();
        public Task InitializeAsync();
    }
}
