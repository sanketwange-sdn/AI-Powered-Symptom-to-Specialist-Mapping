using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Application.Service
{

    using global::App.Application.Interfaces.Services;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    namespace App.Application.Services
    {
        public class DoctorRagService : IDoctorRagService
        {
            // Dummy doctor database for demonstration
            private static readonly List<Doctor> Doctors = new()
        {
            new Doctor { Name = "Dr. Alice Smith", Specialty = "Cardiologist", Availability = "Mon 10am, Wed 2pm" },
            new Doctor { Name = "Dr. Bob Jones", Specialty = "Dermatologist", Availability = "Tue 1pm, Thu 9am" },
            new Doctor { Name = "Dr. Carol Lee", Specialty = "Neurologist", Availability = "Fri 11am" },
            new Doctor { Name = "Dr. David Kim", Specialty = "Gastroenterologist", Availability = "Mon 3pm, Thu 4pm" },
            new Doctor { Name = "Dr. Eva Patel", Specialty = "General Practitioner", Availability = "Everyday 8am-5pm" }
        };

            public Task<List<Doctor>> RetrieveMatchingDoctorsAsync(string specialist, string urgencyLevel)
            {
                // Simple filter by specialty. In production, use vector search and consider urgency.
                var matchingDoctors = Doctors
                    .Where(d => d.Specialty.Equals(specialist, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // If no doctors found, fallback to General Practitioner
                if (!matchingDoctors.Any())
                {
                    matchingDoctors = Doctors
                        .Where(d => d.Specialty.Equals("General Practitioner", System.StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return Task.FromResult(matchingDoctors);
            }
        }
    }

}
