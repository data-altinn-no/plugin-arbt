using ES_ARBT_V3.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Nadobe;
using Nadobe.Common.Interfaces;
using Nadobe.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ES_ARBT_V3
{
    public class Metadata
    {
        private ApplicationSettings _settings;

        public Metadata(IApplicationSettings settings)
        {
            _settings = (ApplicationSettings)settings;
        }



        public List<EvidenceCode> GetEvidenceCodes()
        {
            var a = new List<EvidenceCode>();

            return a;
        }
    }

    public class EvidenceSourceMetadata : IEvidenceSourceMetadata
    {
        public const string SourceEnhetsregisteret = "Arbeidstilsynet";

        public const int ERROR_ORGANIZATION_NOT_FOUND = 1;

        public const int ERROR_CCR_UPSTREAM_ERROR = 2;

        public const int ERROR_NO_REPORT_AVAILABLE = 3;

        public const int ERROR_ASYNC_REQUIRED_PARAMS_MISSING = 4;

        public const int ERROR_ASYNC_ALREADY_INITIALIZED = 5;

        public const int ERROR_ASYNC_NOT_INITIALIZED = 6;

        public const int ERROR_AYNC_STATE_STORAGE = 7;

        public const int ERROR_ASYNC_HARVEST_NOT_AVAILABLE = 8;

        public const int ERROR_CERTIFICATE_OF_REGISTRATION_NOT_AVAILABLE = 9;

        private ApplicationSettings _settings;

        public EvidenceSourceMetadata(IApplicationSettings settings)
        {
            _settings = (ApplicationSettings)settings;
        }

        public List<EvidenceCode> GetEvidenceCodes()
        {
            return (new Metadata(_settings)).GetEvidenceCodes();
        }
    }
}
