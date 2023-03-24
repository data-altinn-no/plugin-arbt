namespace Altinn.Dan.Plugin.Arbeidstilsynet.Models
{
    public class BilpleieArbt
    {
        public Data data { get; set; }
    }

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
