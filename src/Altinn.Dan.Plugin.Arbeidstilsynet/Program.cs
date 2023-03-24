using Altinn.Dan.Plugin.Arbeidstilsynet.Config;
using Dan.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
        .ConfigureDanPluginDefaults()
        .ConfigureServices((context, services) =>
        {
            services.Configure<Settings>(context.Configuration);
        })
        .Build();
await host.RunAsync();



