using App.Application.Interfaces;
using App.Application.Interfaces.Repositories;
using App.Domain.Entities;
using App.Infrastructure.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Infrastructure.Repository
{
    public class DoctorRepository(ApplicationDbContext context, IDbConnectionFactory dbConnectionFactory) : BaseRepository<Doctor>(context, dbConnectionFactory), IDoctorRepository
    {
        private readonly List<Doctor> _doctors = new()
    {
        new Doctor { Id = 1, Name = "Dr. Alice Smith", Specialty = "Cardiologist"},
        new Doctor { Id = 2, Name = "Dr. Bob Jones", Specialty = "Dermatologist"},
        new Doctor { Id = 3, Name = "Dr. Carol Lee", Specialty = "Neurologist"},
        new Doctor { Id = 4, Name = "Dr. David Kim", Specialty = "Gastroenterologist"},
        new Doctor { Id = 5, Name = "Dr. Eva Patel", Specialty = "General Practitioner" },
        new Doctor { Id = 6, Name = "Dr. Frank White", Specialty = "Orthopedic Surgeon"},
        new Doctor { Id = 7, Name = "Dr. Grace Martinez", Specialty = "Pediatrician"},
        new Doctor { Id = 8, Name = "Dr. Henry Wilson", Specialty = "Oncologist"},
        new Doctor { Id = 9, Name = "Dr. Isabella Lopez", Specialty = "Ophthalmologist"},
        new Doctor { Id = 10, Name = "Dr. Jack Thompson", Specialty = "Psychiatrist"},
        new Doctor { Id = 11, Name = "Dr. Karen Brown", Specialty = "Endocrinologist"},
        new Doctor { Id = 12, Name = "Dr. Liam Scott", Specialty = "Rheumatologist"},
        new Doctor { Id = 13, Name = "Dr. Mia Davis", Specialty = "Obstetrician/Gynecologist"},
        new Doctor { Id = 14, Name = "Dr. Noah Clark", Specialty = "Pulmonologist"},
        new Doctor { Id = 15, Name = "Dr. Olivia Harris", Specialty = "Urologist"},
        new Doctor { Id = 16, Name = "Dr. Paul Wright", Specialty = "Nephrologist"},
        new Doctor { Id = 17, Name = "Dr. Quinn Adams", Specialty = "Allergist/Immunologist"},
        new Doctor { Id = 18, Name = "Dr. Rachel Green", Specialty = "Hematologist"},
        new Doctor { Id = 19, Name = "Dr. Samuel Turner", Specialty = "Plastic Surgeon"},
        new Doctor { Id = 20, Name = "Dr. Tina Nguyen", Specialty = "Family Medicine"},
        new Doctor { Id = 21, Name = "Dr. Kunal Wadibhasme", Specialty = "Cardiologist"},
    };

        public Task<List<Doctor>> GetAllDoctorsAsync()
        {
            return Task.FromResult(_doctors);
        }

        public Task<List<Doctor>> GetDoctorsBySpecialtyAsync(string specialty)
        {
            var matchingDoctors = _doctors
                .Where(d => d.Specialty.Equals(specialty, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Task.FromResult(matchingDoctors);
        }
    }
}
