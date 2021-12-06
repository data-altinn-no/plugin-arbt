using System;

namespace Altinn.Dan.Plugin.Arbeidstilsynet.Config
{
    public class ApplicationSettings : IApplicationSettings
    {
        public static ApplicationSettings ApplicationConfig;

        public ApplicationSettings()
        {
            ApplicationConfig = this;
        }

        public string BemanningUrl
        {
            get { return Environment.GetEnvironmentVariable("BemanningUrl"); }
        }

        public string RenholdUrl
        {
            get { return Environment.GetEnvironmentVariable("RenholdUrl"); }
        }
    }
}
