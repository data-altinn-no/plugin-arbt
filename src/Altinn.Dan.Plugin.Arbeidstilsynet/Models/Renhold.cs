using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.Dan.Plugin.Arbeidstilsynet.Models
{
    public class Renhold
    {
        public DateTime StatusEndret { get; set; }
        public string Organisasjonsnummer { get; set; }
        public string Status { get; set; }
    }
}

