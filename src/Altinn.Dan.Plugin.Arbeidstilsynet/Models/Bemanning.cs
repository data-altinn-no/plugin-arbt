using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Altinn.Dan.Plugin.Arbeidstilsynet.Models
{
    public class Bemanning
    {
        [JsonProperty("organisasjonsnummer")]
        public string Organisasjonsnummer { get; set; }

        [JsonProperty("godkjenningsstatus")]
        public string Godkjenningsstatus { get; set; }
    }
}

