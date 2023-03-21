using Altinn.Dan.Plugin.Arbeidstilsynet.Config;
using Dan.Common.Compat;
using Dan.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Text.Json.Serialization;

var host = new HostBuilder()
        .ConfigureDanPluginDefaults()
        .ConfigureServices((context, services) =>
        {
            services.AddHttpClient();
            services.Configure<ApplicationSettings>(context.Configuration);
            // services.AddSingleton<IEvidenceSourceMetadata, EvidenceSourceMetadata>();
            services.Configure<JsonSerializerOptions>(options =>
            {
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.Converters.Add(new JsonStringEnumConverter());
                options.Converters.Add(new AuthorizationRequirementJsonConverter());
            });
        })
        .Build();
await host.RunAsync();
        
    

