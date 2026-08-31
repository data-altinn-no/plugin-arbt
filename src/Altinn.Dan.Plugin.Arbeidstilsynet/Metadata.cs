using Altinn.Dan.Plugin.Arbeidstilsynet.Models;
using Dan.Common.Enums;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Altinn.Dan.Plugin.Arbeidstilsynet
{
    public class EvidenceSourceMetadata : IEvidenceSourceMetadata
    {
        public const string SOURCE = "Arbeidstilsynet";
        public const int ERROR_ORGANIZATION_NOT_FOUND = 1;
        public const int ERROR_CCR_UPSTREAM_ERROR = 2;
        private const string SERIVCECONTEXT_EBEVIS = "eBevis";
        private const string ARBT = "Arbeidstilsynet";

        public List<EvidenceCode> GetEvidenceCodes()
        {
            return new List<EvidenceCode>()
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
                            EvidenceValueName = "organisasjonsnummer",
                            ValueType = EvidenceValueType.String,
                            Source = ARBT
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "godkjenningsstatus",
                            ValueType = EvidenceValueType.String,
                            Source = ARBT
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "default",
                            ValueType = EvidenceValueType.JsonSchema,
                            JsonSchemaDefintion = EvidenceValue.SchemaFromObject<Bemanning>(Formatting.Indented),
                            Source = ARBT
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
                            EvidenceValueName = "organisasjonsnummer",
                            ValueType = EvidenceValueType.String,
                            Source = ARBT
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "status",
                            ValueType = EvidenceValueType.String,
                            Source = ARBT
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "statusEndret",
                            ValueType = EvidenceValueType.DateTime,
                            Source = ARBT
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "default",
                            ValueType = EvidenceValueType.JsonSchema,
                            JsonSchemaDefintion = EvidenceValue.SchemaFromObject<Renhold>(Formatting.Indented),
                            Source = ARBT
                        }
                    }
                },
                new EvidenceCode()
                {
                    EvidenceCodeName = "Bilpleieregisteret",
                    EvidenceSource = EvidenceSourceMetadata.SOURCE,
                    BelongsToServiceContexts = new List<string>() { SERIVCECONTEXT_EBEVIS },
                    Values = new List<EvidenceValue>()
                    {
                        new EvidenceValue()
                        {
                            EvidenceValueName = "organisasjonsnummer",
                            ValueType = EvidenceValueType.String,
                            Source = ARBT
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "registerstatus",
                            ValueType = EvidenceValueType.String,
                            Source = ARBT
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "registerstatusTekst",
                            ValueType = EvidenceValueType.String,
                            Source = ARBT
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "godkjenningsstatus",
                            ValueType = EvidenceValueType.String,
                            Source = ARBT
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "default",
                            ValueType = EvidenceValueType.JsonSchema,
                            JsonSchemaDefintion = EvidenceValue.SchemaFromObject<BilpleieregisterResult>(Formatting.Indented),
                            Source = ARBT
                        }
                    }
                }
            };
        }
    }
}
