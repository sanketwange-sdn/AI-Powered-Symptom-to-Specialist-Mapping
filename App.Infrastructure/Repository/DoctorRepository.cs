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
        new Doctor { Id = 5, Name = "Dr. Eva Patel", Specialty = "General Practitioner" }
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
