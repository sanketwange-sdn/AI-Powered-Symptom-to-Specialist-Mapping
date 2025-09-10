using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Application.Interfaces.Services
{

    // We are building a robust .NET Core application service that performs the following flow:
    // 1. Accepts free-text symptom descriptions from the user.
    // 2. Uses Google Gemini API to extract structured data from the symptoms text:
    //    - Suggested specialist type (e.g., Cardiologist, Dermatologist).
    //    - Urgency level (e.g., Urgent, Routine).
    // 3. Maps the extracted symptoms against a predefined medical taxonomy database to improve accuracy.
    // 4. Uses Retrieval-Augmented Generation (RAG) to search a local or vector database of doctors:
    //    - Retrieve doctors matching the specialist and urgency level.
    //    - Provide doctor availability and detailed specialties.
    // 5. Returns a complete structured response including:
    //    - Specialist suggestion.
    //    - Urgency level.
    //    - List of doctors with name, specialty, and available times.
    //
    // Design should include:
    // - ISymptomAnalysisService interface and its implementation SymptomAnalysisService.
    // - IMedicalTaxonomyService interface for mapping symptoms to specialist.
    // - IDoctorRagService interface for RAG-based doctor search.
    // - SymptomAnalysisResult model class.
    // - Doctor model class.
    // The service should read configuration (API key, endpoint) from appsettings.json.
    //
    // Include proper exception handling and basic logging.

    public interface ISymptomAnalysisService
    {
        Task<SymptomAnalysisResult> AnalyzeSymptomsAsync(string symptoms);
    }

    public interface IMedicalTaxonomyService
    {
        Task<string> MapSymptomsToSpecialistAsync(List<string> extractedSymptoms);
    }

    public interface IDoctorRagService
    {
        Task<List<Doctor>> RetrieveMatchingDoctorsAsync(string specialist, string urgencyLevel);
    }

    public class SymptomAnalysisResult
    {
        public string SpecialistSuggestion { get; set; }
        public string UrgencyLevel { get; set; }
        public List<Doctor> Doctors { get; set; }
    }

    public class Doctor
    {
        public string Name { get; set; }
        public string Specialty { get; set; }
        public string Availability { get; set; }
    }
}
