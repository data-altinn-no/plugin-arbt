using System;
using System.Collections.Generic;
using System.Text;

namespace ES_ARBT_V3.Config
{
    public interface IApplicationSettings
    {
        string RedisConnectionString { get; }
        TimeSpan Breaker_RetryWaitTime { get; }
        TimeSpan Breaker_OpenCircuitTime { get; }
        bool IsTest { get; }
        string BemanningUrl { get; }

        string RenholdUrl { get; }


    }
}
