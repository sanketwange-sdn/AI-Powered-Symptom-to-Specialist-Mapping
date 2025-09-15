using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.Entities
{
    public class Doctor : BaseEntity
    {
        public int Id { get; set; }  // Database Primary Key
        public string Name { get; set; }
        public string Specialty { get; set; }
    }
}
