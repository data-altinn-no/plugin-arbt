using Altinn.Dan.Plugin.Arbeidstilsynet.Config;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nadobe.Common.Interfaces;
using Polly;
using Polly.Caching.Distributed;
using Polly.Extensions.Http;
using Polly.Registry;
using System;
using System.Threading.Tasks;

namespace Altinn.Dan.Plugin.Arbeidstilsynet
{
    class Program
    {
        private static IApplicationSettings ApplicationSettings { get; set; }

        private static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddHttpClient();

                    services.AddSingleton<IApplicationSettings, ApplicationSettings>();
                    services.AddSingleton<IEvidenceSourceMetadata, EvidenceSourceMetadata>();

                    ApplicationSettings = services.BuildServiceProvider().GetRequiredService<IApplicationSettings>();

                    // Client configured with circuit breaker policies
                    services.AddHttpClient("SafeHttpClient", client => { client.Timeout = new TimeSpan(0, 0, 30); });                       
                })
                .Build();
            return host.RunAsync();
        }
    }
}
