using Altinn.Dan.Plugin.Arbeidstilsynet.Models.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Altinn.Dan.Plugin.Arbeidstilsynet.Models
{
    public class BilpleieArbt
    {
        //public Metadata metadata { get; set; }
        public Data data { get; set; }
    }
    /*
    public class Metadata
    {
        public string versjon { get; set; }
        public DateTime datoTidGenerert { get; set; }
    } */

    public class Data
    {
        public string organisasjonsnummer { get; set; }
        public int registerstatus { get; set; }
        public string registerstatusTekst { get; set; }
        public string godkjenningsstatus { get; set; }
        public Enhet[] underenheter { get; set; }
    }

    public class Enhet
    {
        public string organisasjonsnummer { get; set; }
        public int registerstatus { get; set; }
        public string registerstatusTekst { get; set; }
        public string godkjenningsstatus { get; set; }
    }

}
