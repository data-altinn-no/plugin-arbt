using Altinn.Dan.Plugin.Arbeidstilsynet.Config;
using Dan.Common.Compat;
using Dan.Common.Extensions;
using Dan.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Altinn.Dan.Plugin.Arbeidstilsynet
{
    class Program
    {
        private static IApplicationSettings ApplicationSettings { get; set; }

        private static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureDanPluginDefaults()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddHttpClient();

                    services.AddSingleton<IApplicationSettings, ApplicationSettings>();
                    services.AddSingleton<IEvidenceSourceMetadata, EvidenceSourceMetadata>();

                    ApplicationSettings = services.BuildServiceProvider().GetRequiredService<IApplicationSettings>();
                    services.AddHttpClient("SafeHttpClient", client => { client.Timeout = new TimeSpan(0, 0, 30); });

                    services.Configure<JsonSerializerOptions>(options =>
                    {
                        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                        options.Converters.Add(new JsonStringEnumConverter());
                        options.Converters.Add(new AuthorizationRequirementJsonConverter());
                    });
                })
                .Build();
            return host.RunAsync();
        }
    }
}
