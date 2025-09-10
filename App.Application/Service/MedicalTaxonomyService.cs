using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Application.Interfaces.Services;

namespace App.Application.Service
{
    public class MedicalTaxonomyService : IMedicalTaxonomyService
    {
        // Dummy mapping for demonstration. In production, use a database or more advanced logic.
        private static readonly Dictionary<string, string> SymptomToSpecialist = new()
        {
            { "chest pain", "Cardiologist" },
            { "shortness of breath", "Cardiologist" },
            { "skin rash", "Dermatologist" },
            { "headache", "Neurologist" },
            { "abdominal pain", "Gastroenterologist" }
        };

        public Task<string> MapSymptomsToSpecialistAsync(List<string> extractedSymptoms)
        {
            foreach (var symptom in extractedSymptoms)
            {
                if (SymptomToSpecialist.TryGetValue(symptom.ToLowerInvariant(), out var specialist))
                {
                    return Task.FromResult(specialist);
                }
            }
            // Default fallback
            return Task.FromResult("General Practitioner");
        }
    }
}