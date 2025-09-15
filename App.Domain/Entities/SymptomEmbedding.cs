using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace App.Domain.Entities
{
    public class SymptomEmbedding : BaseEntity
    {
        public int Id { get; set; }
        public string Symptom { get; set; }
        public string Specialist { get; set; }
        public float[] EmbeddingVector { get; set; }
    }
}
