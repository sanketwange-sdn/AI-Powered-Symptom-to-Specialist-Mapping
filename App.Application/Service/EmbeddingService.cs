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
        // Cardiologist
    { "chest pain", "Cardiologist" },
    { "shortness of breath", "Cardiologist" },
    { "irregular heartbeat", "Cardiologist" },
    { "high blood pressure", "Cardiologist" },
    { "swelling in legs", "Cardiologist" },
    { "dizziness with exertion", "Cardiologist" },
    { "palpitations", "Cardiologist" },

    // Neurologist
    { "headache", "Neurologist" },
    { "migraine", "Neurologist" },
    { "seizures", "Neurologist" },
    { "memory loss", "Neurologist" },
    { "confusion", "Neurologist" },
    { "numbness", "Neurologist" },
    { "weakness on one side", "Neurologist" },
    { "slurred speech", "Neurologist" },
    { "dizziness", "Neurologist" },
    { "tremors", "Neurologist" },
    { "difficulty walking", "Neurologist" },

    // Dermatologist
    { "skin rash", "Dermatologist" },
    { "itching", "Dermatologist" },
    { "acne", "Dermatologist" },
    { "eczema", "Dermatologist" },
    { "psoriasis", "Dermatologist" },
    { "hives", "Dermatologist" },
    { "skin infection", "Dermatologist" },
    { "hair loss", "Dermatologist" },
    { "nail discoloration", "Dermatologist" },
    { "moles changing color", "Dermatologist" },

    // Gastroenterologist
    { "abdominal pain", "Gastroenterologist" },
    { "heartburn", "Gastroenterologist" },
    { "diarrhea", "Gastroenterologist" },
    { "constipation", "Gastroenterologist" },
    { "blood in stool", "Gastroenterologist" },
    { "nausea", "Gastroenterologist" },
    { "vomiting", "Gastroenterologist" },
    { "jaundice", "Gastroenterologist" },
    { "bloating", "Gastroenterologist" },

    // Pulmonologist
    { "cough", "Pulmonologist" },
    { "chronic cough", "Pulmonologist" },
    { "asthma", "Pulmonologist" },
    { "wheezing", "Pulmonologist" },
    { "lung infection", "Pulmonologist" },
    { "chest tightness", "Pulmonologist" },
    { "difficulty breathing", "Pulmonologist" },
    { "sleep apnea", "Pulmonologist" },

    // Endocrinologist
    { "diabetes", "Endocrinologist" },
    { "thyroid swelling", "Endocrinologist" },
    { "excessive thirst", "Endocrinologist" },
    { "excessive urination", "Endocrinologist" },
    { "sudden weight loss", "Endocrinologist" },
    { "sudden weight gain", "Endocrinologist" },
    { "fatigue", "Endocrinologist" },
    { "hair thinning", "Endocrinologist" },

    // Orthopedic
    { "joint pain", "Orthopedic" },
    { "knee pain", "Orthopedic" },
    { "back pain", "Orthopedic" },
    { "shoulder pain", "Orthopedic" },
    { "fracture", "Orthopedic" },
    { "sports injury", "Orthopedic" },
    { "stiff joints", "Orthopedic" },
    { "bone pain", "Orthopedic" },
    { "arthritis", "Orthopedic" },

    // Urologist
    { "frequent urination", "Urologist" },
    { "painful urination", "Urologist" },
    { "blood in urine", "Urologist" },
    { "kidney stones", "Urologist" },
    { "difficulty urinating", "Urologist" },
    { "incontinence", "Urologist" },
    { "testicular pain", "Urologist" },

    // Gynecologist
    { "irregular periods", "Gynecologist" },
    { "menstrual cramps", "Gynecologist" },
    { "pregnancy symptoms", "Gynecologist" },
    { "pelvic pain", "Gynecologist" },
    { "infertility", "Gynecologist" },
    { "menopause symptoms", "Gynecologist" },
    { "vaginal discharge", "Gynecologist" },

    // Ophthalmologist
    { "blurred vision", "Ophthalmologist" },
    { "eye pain", "Ophthalmologist" },
    { "red eyes", "Ophthalmologist" },
    { "watery eyes", "Ophthalmologist" },
    { "double vision", "Ophthalmologist" },
    { "vision loss", "Ophthalmologist" },
    { "eye strain", "Ophthalmologist" },
    { "cataract", "Ophthalmologist" },
    { "glaucoma", "Ophthalmologist" },

    // ENT Specialist
    { "ear pain", "ENT Specialist" },
    { "hearing loss", "ENT Specialist" },
    { "sore throat", "ENT Specialist" },
    { "tonsillitis", "ENT Specialist" },
    { "sinus pain", "ENT Specialist" },
    { "nasal congestion", "ENT Specialist" },
    { "tinnitus", "ENT Specialist" },
    { "dizziness related to ear", "ENT Specialist" },

    // Psychiatrist
    { "depression", "Psychiatrist" },
    { "anxiety", "Psychiatrist" },
    { "insomnia", "Psychiatrist" },
    { "panic attacks", "Psychiatrist" },
    { "hallucinations", "Psychiatrist" },
    { "suicidal thoughts", "Psychiatrist" },
    { "bipolar symptoms", "Psychiatrist" },
    { "addiction", "Psychiatrist" },

    // General fallback
    { "fever", "General Practitioner" },
    { "fatigue without cause", "General Practitioner" },
    { "loss of appetite", "General Practitioner" },
    { "weakness", "General Practitioner" },
    { "unexplained pain", "General Practitioner" },
    { "general illness", "General Practitioner" }
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

            // Get the wwwroot path
            var currentDir = Directory.GetCurrentDirectory();
            var wwwrootPath = Path.Combine(currentDir, "wwwroot");
            // Combine path to symptom_embeddings.json
            var path = Path.Combine(wwwrootPath, "symptom_embeddings.json");

            var json = File.ReadAllText(path);
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
