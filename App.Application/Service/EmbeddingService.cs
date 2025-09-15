using App.Application.Interfaces.Services;
using App.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace App.Application.Service
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _modelId;

        public EmbeddingService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["HuggingFace:ApiKey"];
            _modelId = configuration["HuggingFace:ModelId"];
        }

        public async Task<List<float[]>> ComputeEmbeddingsBatchAsync(List<string> texts)
        {
            var requestBody = new
            {
                texts = texts,
                model = "nomic-embed-text-v1",
                task_type = "search_document"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer nk-EWa00WToJ5-GinQORoCPVyJWYgPqwY7NXU6U6x_w750");

            var response = await _httpClient.PostAsync("https://api-atlas.nomic.ai/v1/embedding/text", content);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;

            var embeddingsElement = root.GetProperty("embeddings");

            var embeddings = new List<float[]>();

            foreach (var embeddingArray in embeddingsElement.EnumerateArray())
            {
                var embeddingList = new List<float>();
                foreach (var number in embeddingArray.EnumerateArray())
                {
                    embeddingList.Add(number.GetSingle());
                }
                embeddings.Add(embeddingList.ToArray());
            }

            return embeddings;
        }

        public async Task GenerateAndSaveEmbeddingsFileAsync(string filePath)
        {
            var symptomToSpecialistMap = new Dictionary<string, string>
    {
        { "chest pain", "Cardiologist" },
        { "skin rash", "Dermatologist" },
        { "headache", "Neurologist" }
        // Add more as needed
    };

            var symptomTexts = symptomToSpecialistMap.Keys.ToList();

            // Compute embeddings in batch
            var embeddings = await ComputeEmbeddingsBatchAsync(symptomTexts);

            var symptomEmbeddings = new List<SymptomEmbedding>();

            for (int i = 0; i < symptomTexts.Count; i++)
            {
                symptomEmbeddings.Add(new SymptomEmbedding
                {
                    Id = i + 1,
                    Symptom = symptomTexts[i],
                    Specialist = symptomToSpecialistMap[symptomTexts[i]],
                    EmbeddingVector = embeddings[i]
                });
            }

            var json = JsonSerializer.Serialize(symptomEmbeddings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }

        public Task<List<SymptomEmbedding>> GetAllEmbeddingsAsync()
        {
            var json = File.ReadAllText("D:\\SmartData\\AI Assignment\\dotnetsmartPODBackendInitialArch\\App.Api\\wwwroot\\symptom_embeddings.json");
            var embeddings = JsonSerializer.Deserialize<List<SymptomEmbedding>>(json);
            return Task.FromResult(embeddings);
        }

        //public async Task<List<float[]>> ComputeEmbeddingsBatchAsync(List<string> texts)
        //{
        //    var request = new HttpRequestMessage
        //    {
        //        Method = HttpMethod.Post,
        //        RequestUri = new Uri("https://api-inference.huggingface.co/models/sentence-transformers/all-MiniLM-L6-v2"),
        //        Headers =
        //{
        //    { "Authorization", "Bearer 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" },
        //},
        //        Content = new StringContent(JsonSerializer.Serialize(texts), Encoding.UTF8, "application/json"),
        //    };

        //    using var response = await _httpClient.SendAsync(request);
        //    response.EnsureSuccessStatusCode();

        //    var responseString = await response.Content.ReadAsStringAsync();

        //    // The API returns an array of float arrays
        //    var embeddings = JsonSerializer.Deserialize<List<float[]>>(responseString);

        //    return embeddings;
        //}

    }
}
