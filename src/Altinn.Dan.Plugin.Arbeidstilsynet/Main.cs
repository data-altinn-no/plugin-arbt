using Altinn.Dan.Plugin.Arbeidstilsynet.Config;
using Altinn.Dan.Plugin.Arbeidstilsynet.Models;
using Altinn.Dan.Plugin.Arbeidstilsynet.Utils;
using Dan.Common;
using Dan.Common.Exceptions;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Altinn.Dan.Plugin.Arbeidstilsynet
{
    public class Main
    {
        private readonly ILogger _logger;
        private readonly HttpClient _client;
        private readonly ApplicationSettings _settings;
        private readonly IEvidenceSourceMetadata _metadata;
        private readonly IEntityRegistryService _entityRegistryService;

        private const int NOT_FOUND = 1001;

        public Main(IHttpClientFactory httpClientFactory, IOptions<ApplicationSettings> settings, IEvidenceSourceMetadata evidenceSourceMetadata, ILoggerFactory loggerFactory, IEntityRegistryService entityRegistryService)
        {
            _client = httpClientFactory.CreateClient("SafeHttpClient");
            _client.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", "nadobe/data.altinn.no");
            _settings = settings.Value;
            _metadata = evidenceSourceMetadata;
            _logger = loggerFactory.CreateLogger<Main>();
            _entityRegistryService = entityRegistryService;
        }

        [Function("Bemanningsforetakregisteret")]
        public async Task<HttpResponseData> Bemanning(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestData req, FunctionContext context)
        {
            _logger.LogInformation("Running func 'Bemanning'");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesBemanning(evidenceHarvesterRequest));
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesBemanning(EvidenceHarvesterRequest evidenceHarvesterRequest)
        {
            BREntityRegisterEntry actualOrganization = null;

            if (_settings.IsTest)
            {
                actualOrganization = new BREntityRegisterEntry() { Organisasjonsnummer = long.Parse(evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber) };
            }
            else
            {
                //The registry only has main units, so we have to traverse the enterprise structure in case the subject of the lookup is a subunit
                actualOrganization = await BRUtils.GetMainUnit(evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber, _client);
            }

            var bemanning = await Helpers.MakeRequest<Bemanning>(_client, string.Format(_settings.BemanningUrl, actualOrganization.Organisasjonsnummer), actualOrganization.Organisasjonsnummer.ToString(), _logger);

            var ecb = new EvidenceBuilder(_metadata, "Bemanningsforetakregisteret");
            ecb.AddEvidenceValue($"Organisasjonsnummer", bemanning.Organisasjonsnummer, EvidenceSourceMetadata.SOURCE);
            ecb.AddEvidenceValue($"Godkjenningsstatus", bemanning.Godkjenningsstatus, EvidenceSourceMetadata.SOURCE);

            return ecb.GetEvidenceValues();
        }

        [Function("Renholdsregisteret")]
        public async Task<HttpResponseData> Renhold(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestData req, FunctionContext context)
        {
            _logger.LogInformation("Running func 'Renhold'");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return  await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesRenhold(evidenceHarvesterRequest));
        }

        [Function("Bilpleieregisteret")]
        public async Task<HttpResponseData> Bilpleie(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestData req, FunctionContext context)
        {
            _logger.LogInformation("Running func 'Bilpleie'");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesBilpleie(evidenceHarvesterRequest));
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesBilpleie(EvidenceHarvesterRequest evidenceHarvesterRequest)
        {
            BilpleieArbt result = null;
            Enhet unit = null;
            //The registry only has main units, so we have to traverse the enterprise structure in case the subject of the lookup is a subunit
            string actualOrgNo = null;

            if (_settings.IsTest)
            {
                //No tenor, no ER
                actualOrgNo = evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber;
            }
            else
            {
                var main = await BRUtils.GetMainUnit(evidenceHarvesterRequest.OrganizationNumber, _client);
                actualOrgNo = main.Organisasjonsnummer.ToString();
            }

            result = await Helpers.MakeRequest<BilpleieArbt>(_client, string.Format(_settings.BilpleieURl, actualOrgNo), evidenceHarvesterRequest.OrganizationNumber, _logger);

            if (actualOrgNo != evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber) //subject is not main unit
            {
                unit = result.data.underenheter.FirstOrDefault(x => x.organisasjonsnummer == evidenceHarvesterRequest.OrganizationNumber);

                if (unit != null)
                    return CreateEvidenceResponse(unit.organisasjonsnummer, unit.registerstatus, unit.registerstatusTekst, unit.godkjenningsstatus);
                else
                    throw new EvidenceSourcePermanentServerException(NOT_FOUND, "Not found");
            }
            else
            {
                return CreateEvidenceResponse(result.data.organisasjonsnummer, result.data.registerstatus, result.data.registerstatusTekst, result.data.godkjenningsstatus);
            }

        }

        private List<EvidenceValue> CreateEvidenceResponse(string orgno, int registerstatus, string registerstatustekst, string godkjenningsstatus)
        {
            var ecb = new EvidenceBuilder(_metadata, "Bilpleieregisteret");
            ecb.AddEvidenceValue("organisasjonsnummer", orgno, EvidenceSourceMetadata.SOURCE);
            ecb.AddEvidenceValue("registerstatus", registerstatus, EvidenceSourceMetadata.SOURCE);
            ecb.AddEvidenceValue("registerstatusTekst", registerstatustekst, EvidenceSourceMetadata.SOURCE);
            ecb.AddEvidenceValue("godkjenningsstatus", godkjenningsstatus, EvidenceSourceMetadata.SOURCE);

            return ecb.GetEvidenceValues();
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesRenhold(EvidenceHarvesterRequest evidenceHarvesterRequest)
        {
            BREntityRegisterEntry actualOrganization = null;

            if (_settings.IsTest)
            {
                actualOrganization = new BREntityRegisterEntry() { Organisasjonsnummer = long.Parse(evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber) };
            }
            else
            {
                //The registry only has main units, so we have to traverse the enterprise structure in case the subject of the lookup is a subunit
                actualOrganization = await BRUtils.GetMainUnit(evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber, _client);
            }

            var content = await Helpers.MakeRequest<Renhold>(_client, string.Format(_settings.RenholdUrl, actualOrganization.Organisasjonsnummer), actualOrganization.Organisasjonsnummer.ToString(), _logger);

            var ecb = new EvidenceBuilder(_metadata, "Renholdsregisteret");
            ecb.AddEvidenceValue($"Organisasjonsnummer", content.Organisasjonsnummer, EvidenceSourceMetadata.SOURCE);
            ecb.AddEvidenceValue($"Status", content.Status, EvidenceSourceMetadata.SOURCE);

            var statusChanged = Convert.ToDateTime(content.StatusEndret);

            if (statusChanged != DateTime.MinValue)
            {
                ecb.AddEvidenceValue($"StatusEndret", statusChanged, EvidenceSourceMetadata.SOURCE, false);
            }

            return ecb.GetEvidenceValues();
        }

        [Function(Constants.EvidenceSourceMetadataFunctionName)]
        public async Task<HttpResponseData> Metadata(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req, FunctionContext context)
        {
            _logger.LogInformation($"Running metadata for {Constants.EvidenceSourceMetadataFunctionName}");
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(_metadata.GetEvidenceCodes());
            return response;
        }
    }
}
