namespace Altinn.Dan.Plugin.Arbeidstilsynet.Config
{
    public class ApplicationSettings
    {
        public static ApplicationSettings ApplicationConfig;

        public ApplicationSettings()
        {
            ApplicationConfig = this;
        }

        public string BemanningUrl { get; set; }


        public string RenholdUrl { get; set; }  

        public string BilpleieURl { get; set; }

        public bool IsTest { get; set; }
    }
}
