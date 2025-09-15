using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Application.Service
{


    namespace App.Application.Services
    {
        public class DoctorRagService : IDoctorRagService
        {
            private readonly IDoctorRepository _doctorRepository;


            public DoctorRagService(IDoctorRepository doctorRepository)
            {
                _doctorRepository = doctorRepository;
            }

            public async Task<List<Doctor>> RetrieveMatchingDoctorsAsync(string specialist, string urgencyLevel)
            {
                List<Doctor> doctors = await _doctorRepository.GetDoctorsBySpecialtyAsync(specialist);

                if (urgencyLevel.Equals("Urgent", StringComparison.OrdinalIgnoreCase))
                {
                    //get doctors by availability for now its not implimented
                    doctors = doctors
                        .ToList();
                }

                if (!doctors.Any())
                {
                    doctors = await _doctorRepository.GetDoctorsBySpecialtyAsync("General Practitioner");
                }

                return doctors;
            }

        }
    }

}
