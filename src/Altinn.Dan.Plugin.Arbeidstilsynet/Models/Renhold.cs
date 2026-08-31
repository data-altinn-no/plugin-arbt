using System;
using Newtonsoft.Json;

namespace Altinn.Dan.Plugin.Arbeidstilsynet.Models
{
    public class Renhold
    {
        [JsonProperty("statusEndret")]
        public DateTime StatusEndret { get; set; }

        [JsonProperty("organisasjonsnummer")]
        public string Organisasjonsnummer { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}

