using Altinn.Dan.Plugin.Arbeidstilsynet.Config;
using Nadobe.Common.Interfaces;
using Nadobe.Common.Models;
using Nadobe.Common.Models.Enums;
using System.Collections.Generic;

namespace Altinn.Dan.Plugin.Arbeidstilsynet
{
    public class Metadata
    {
        private ApplicationSettings _settings;
        private const string SERIVCECONTEXT_EBEVIS = "eBevis";

        public Metadata(IApplicationSettings settings)
        {
            _settings = (ApplicationSettings)settings;
        }

        public List<EvidenceCode> GetEvidenceCodes()
        {
            var a = new List<EvidenceCode>()
            {
                new EvidenceCode()
                {
                    EvidenceCodeName = "Bemanningsforetakregisteret",
                    EvidenceSource = EvidenceSourceMetadata.SOURCE,
                    BelongsToServiceContexts = new List<string>() { SERIVCECONTEXT_EBEVIS },
                    Values = new List<EvidenceValue>()
                    {
                        new EvidenceValue()
                        {
                            EvidenceValueName = "Organisasjonsnummer",
                            ValueType = EvidenceValueType.String
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "Godkjenningsstatus",
                            ValueType = EvidenceValueType.String
                        }
                    }
                },
                new EvidenceCode()
                {
                    EvidenceCodeName = "Renholdsregisteret",
                    EvidenceSource = EvidenceSourceMetadata.SOURCE,
                    BelongsToServiceContexts = new List<string>() { SERIVCECONTEXT_EBEVIS },
                    Values = new List<EvidenceValue>()
                    {
                        new EvidenceValue()
                        {
                            EvidenceValueName = "Organisasjonsnummer",
                            ValueType = EvidenceValueType.String
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "Status",
                            ValueType = EvidenceValueType.String
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "StatusEndret",
                            ValueType = EvidenceValueType.DateTime
                        }
                    }
                }
            };

            return a;
        }
    }

    public class EvidenceSourceMetadata : IEvidenceSourceMetadata
    {
        public const string SOURCE = "Arbeidstilsynet";

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
