using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lab03_storage.Models
{
    public class Registrations
    {
        public Guid RegistationId { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public string Email { get; set; }
        public string ZipCode { get; set; }
        public int Age { get; set; }
        public bool IsFirstTimer { get; set; }
    }
}