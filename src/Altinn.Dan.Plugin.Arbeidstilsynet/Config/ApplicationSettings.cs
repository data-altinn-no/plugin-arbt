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

        public string RedisConnectionString
        {
            get
            {
                return Environment.GetEnvironmentVariable("RedisConnectionString");
            }
        }

        public bool IsTest
        {
            get
            {
                return Environment.GetEnvironmentVariable("IsTest").ToLowerInvariant().Trim() == "true";
            }
        }

        public TimeSpan Breaker_RetryWaitTime
        {
            get
            {
                return TimeSpan.FromSeconds(int.Parse(Environment.GetEnvironmentVariable("Breaker_RetryWaitTime")));
            }
        }

        public TimeSpan Breaker_OpenCircuitTime
        {
            get
            {
                return TimeSpan.FromSeconds(int.Parse(Environment.GetEnvironmentVariable("Breaker_OpenCircuitTime")));
            }
        }

        public string BemanningUrl
        {
            get
            {
                return Environment.GetEnvironmentVariable("BemanningUrl");
            }
        }

        public string RenholdUrl
        {
            get
            {
                return Environment.GetEnvironmentVariable("RenholdUrl");
            }
        }
    }
}
