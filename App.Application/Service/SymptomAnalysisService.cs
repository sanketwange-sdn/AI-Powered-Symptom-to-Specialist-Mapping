using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using App.Application.Interfaces.Services;

namespace App.Application.Services
{
    public class SymptomAnalysisService : ISymptomAnalysisService
    {
        private readonly IMedicalTaxonomyService _medicalTaxonomyService;
        private readonly IDoctorRagService _doctorRagService;
        private readonly string _geminiApiKey;
        private readonly string _geminiEndpoint;

        public SymptomAnalysisService(
            IMedicalTaxonomyService medicalTaxonomyService,
            IDoctorRagService doctorRagService,
            IConfiguration configuration)
        {
            _medicalTaxonomyService = medicalTaxonomyService;
            _doctorRagService = doctorRagService;
            _geminiApiKey = configuration["Gemini:ApiKey"];
            _geminiEndpoint = configuration["Gemini:Endpoint"];
        }

        public async Task<SymptomAnalysisResult> AnalyzeSymptomsAsync(string symptoms)
        {
            try
            {
                Console.WriteLine(_geminiApiKey);
                Console.WriteLine(_geminiEndpoint);
                // 1. Use Gemini API to extract structured data
                var (extractedSymptoms, specialistSuggestion, urgencyLevel) = await ExtractStructuredDataAsync(symptoms);

                // 2. Map symptoms to specialist using taxonomy
                var mappedSpecialist = await _medicalTaxonomyService.MapSymptomsToSpecialistAsync(extractedSymptoms);
                if (!string.IsNullOrWhiteSpace(mappedSpecialist))
                {
                    specialistSuggestion = mappedSpecialist;
                }

                // 3. Retrieve matching doctors using RAG
                var doctors = await _doctorRagService.RetrieveMatchingDoctorsAsync(specialistSuggestion, urgencyLevel);

                // 4. Return structured result
                return new SymptomAnalysisResult
                {
                    SpecialistSuggestion = specialistSuggestion,
                    UrgencyLevel = urgencyLevel,
                    Doctors = doctors
                };
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error analyzing symptoms");
                throw;
            }
        }

        private async Task<(List<string> extractedSymptoms, string specialist, string urgencyLevel)> ExtractStructuredDataAsync(string symptoms)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-goog-api-key", $"{_geminiApiKey}");

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = $"Extract the following from this symptom description: 1) List of symptoms, 2) Most likely specialist type, 3) Urgency level (Urgent, Routine, etc). Respond ONLY in valid JSON format without any markdown, code block, or explanation. Example response: {{ \"symptoms\": [\"chest pain\"], \"specialist\": \"Cardiologist\", \"urgency\": \"Urgent\" }}. Symptom description: \"{symptoms}\""
                            }
                        }
                    }
                }
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_geminiEndpoint, content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            // Gemini API response parsing
            using var doc = System.Text.Json.JsonDocument.Parse(responseString);
            var root = doc.RootElement;

            // Gemini's response format: { "candidates": [ { "content": { "parts": [ { "text": "<json>" } ] } } ] }
            var jsonText = root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            // Parse the returned JSON string
            // Defensive cleanup in case model adds backticks
            if (jsonText.StartsWith("```"))
                jsonText = jsonText.Substring(jsonText.IndexOf('\n') + 1).Trim();
            if (jsonText.EndsWith("```"))
                jsonText = jsonText.Substring(0, jsonText.LastIndexOf("```")).Trim();

            var resultDoc = System.Text.Json.JsonDocument.Parse(jsonText);

            var resultRoot = resultDoc.RootElement;

            var extractedSymptoms = new List<string>();
            if (resultRoot.TryGetProperty("symptoms", out var symptomsElement) && symptomsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var s in symptomsElement.EnumerateArray())
                {
                    if (s.ValueKind == System.Text.Json.JsonValueKind.String)
                        extractedSymptoms.Add(s.GetString());
                }
            }

            var specialist = resultRoot.TryGetProperty("specialist", out var specialistElement) && specialistElement.ValueKind == System.Text.Json.JsonValueKind.String
                ? specialistElement.GetString()
                : string.Empty;

            var urgencyLevel = resultRoot.TryGetProperty("urgency", out var urgencyElement) && urgencyElement.ValueKind == System.Text.Json.JsonValueKind.String
                ? urgencyElement.GetString()
                : string.Empty;

            return (extractedSymptoms, specialist, urgencyLevel);
        }
    }
}
