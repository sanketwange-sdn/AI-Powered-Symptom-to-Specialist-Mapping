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
            // Placeholder for Gemini API call
            // In production, use HttpClient to call Gemini API with _geminiApiKey and _geminiEndpoint
            // Parse response to extract symptoms, specialist, and urgency level

            await Task.Delay(100); // Simulate async call

            // Dummy extraction for demonstration
            var extractedSymptoms = new List<string> { "chest pain", "shortness of breath" };
            var specialist = "Cardiologist";
            var urgencyLevel = "Urgent";

            return (extractedSymptoms, specialist, urgencyLevel);
        }
    }
}
