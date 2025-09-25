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
                if (!extractedSymptoms.Any())
                {
                    // you can return as no symtoms found please try again 
                    return new SymptomAnalysisResult
                    {
                        SpecialistSuggestion = "",
                        UrgencyLevel = "",
                        Doctors = null,
                        UserFriendlyMessage = "Sorry, I couldn’t understand your symptoms. Could you please describe them in a bit more detail?"
                    };
                }

                // 2. Map symptoms to specialist using taxonomy
                var mappedSpecialist = await _medicalTaxonomyService.MapSymptomsToSpecialistAsync(extractedSymptoms);
                if (!string.IsNullOrWhiteSpace(mappedSpecialist))
                {
                    specialistSuggestion = mappedSpecialist;
                }

                // 3. Retrieve matching doctors using RAG
                var doctors = await _doctorRagService.RetrieveMatchingDoctorsAsync(specialistSuggestion, urgencyLevel);

                // 4. Return structured result
                string userFriendlyMessage = "";

                if (doctors != null && doctors.Any())
                {
                    // 1. Serialize the structured data to be sent to Gemini
                    var doctorsJson = System.Text.Json.JsonSerializer.Serialize(doctors);

                    // 2. Craft the prompt for the second Gemini call
                    var finalPrompt = $"You are a medical assistant. Based on the user's symptoms, a {specialistSuggestion} is suggested with an urgency of {urgencyLevel}. " +
                                      $"Here is a list of matching doctors in JSON format: {doctorsJson}. " +
                                      "Please provide a clear and concise summary for the user. Explain the specialist recommendation, the urgency, and list the doctors in a clean, easy-to-read format. " +
                                      "Do not include any JSON or code blocks. Just provide the conversational text.";

                    // 3. Make the Gemini API call
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
                                        text = finalPrompt
                                    }
                                }
                            }
                        }
                    };

                    var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(_geminiEndpoint, content);
                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();

                    // 4. Extract the text from the Gemini response
                    // (This parsing will be similar to your ExtractStructuredDataAsync method)
                    var finalResultDoc = System.Text.Json.JsonDocument.Parse(responseString);

                    var root = finalResultDoc.RootElement;
                    userFriendlyMessage = root
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();
                }
                else
                {
                    userFriendlyMessage = $"Based on your symptoms, we suggest seeing a {specialistSuggestion}. We could not find any doctors in our database that match your request. Please try again later or contact your primary care physician.";
                }


                // 5. Return the structured result with the new user-friendly message
                return new SymptomAnalysisResult
                {
                    SpecialistSuggestion = specialistSuggestion,
                    UrgencyLevel = urgencyLevel,
                    Doctors = doctors,
                    UserFriendlyMessage = userFriendlyMessage 
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
                                text = $"You are a professional medical assistant. Extract only real medical symptoms, specialist type, and urgency from the given description. " +
                           $"If no medical symptoms are present, return {{\"symptoms\": [], \"specialist\": \"\", \"urgency\": \"\"}}. " +
                           $"Respond ONLY in valid JSON format without explanation or markdown. " +
                           $"Example: {{ \"symptoms\": [\"chest pain\"], \"specialist\": \"Cardiologist\", \"urgency\": \"Urgent\" }}. " +
                           $"Symptom description: \"{symptoms}\""
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
