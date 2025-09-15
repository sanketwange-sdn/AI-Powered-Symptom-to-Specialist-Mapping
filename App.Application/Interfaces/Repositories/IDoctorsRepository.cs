using App.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Application.Interfaces.Repositories
{
    public interface IDoctorRepository : IBaseRepository<Doctor>
    {
        Task<List<Doctor>> GetAllDoctorsAsync();
        Task<List<Doctor>> GetDoctorsBySpecialtyAsync(string specialty);
    }
}
