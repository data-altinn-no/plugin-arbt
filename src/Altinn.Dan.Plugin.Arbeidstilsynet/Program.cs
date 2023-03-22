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
            services.Configure<ApplicationSettings>(context.Configuration);
        })
        .Build();
await host.RunAsync();
        
    

