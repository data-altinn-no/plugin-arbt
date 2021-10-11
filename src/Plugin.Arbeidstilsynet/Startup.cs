using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nadobe.Common.Util;
using Polly;
using Polly.Caching;
using Polly.Caching.Distributed;
using Polly.Caching.Serialization.Json;
using Polly.Extensions.Http;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Nadobe.Common.Models;
using Newtonsoft.Json;
using Dan.Plugin.Arbt.Config;
using Dan.Plugin.Arbt;

[assembly: FunctionsStartup(typeof(Dan.Plugin.Arbt.Startup))]

namespace Dan.Plugin.Arbt
{

    public class Startup: FunctionsStartup
    {
        public IApplicationSettings ApplicationSettings { get; private set; }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var Config = new ConfigurationBuilder()
             .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
             .AddEnvironmentVariables()
             .Build();

            ApplicationSettings = Config.Get<ApplicationSettings>();

            builder.Services.AddStackExchangeRedisCache(option =>
            {
                option.Configuration = ApplicationSettings.RedisConnectionString;
            }); 

            builder.Services.AddSingleton<IApplicationSettings>((s) => { return Config.Get<ApplicationSettings>(); });
            builder.Services.AddSingleton<EvidenceSourceMetadata>();

            var distributedCache = builder.Services.BuildServiceProvider().GetRequiredService<IDistributedCache>();

            var registry = new PolicyRegistry()
            {
                { "defaultCircuitBreaker", HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(4, ApplicationSettings.Breaker_RetryWaitTime) },
                { "CachePolicy", Policy.CacheAsync(distributedCache.AsAsyncCacheProvider<string>(), TimeSpan.FromHours(12)) }
            };

            builder.Services.AddPolicyRegistry(registry); 

            // Client configured with circuit breaker policies
            builder.Services.AddHttpClient("SafeHttpClient", client =>
            {
                client.Timeout = new TimeSpan(0,0,30);
            })
            .AddPolicyHandlerFromRegistry("defaultCircuitBreaker");
            
            // Client configured without circuit breaker policies. shorter timeout
            builder.Services.AddHttpClient("CachedHttpClient", client =>
            {
                client.Timeout = new TimeSpan(0, 0, 5);
            });
        }
    }
}
