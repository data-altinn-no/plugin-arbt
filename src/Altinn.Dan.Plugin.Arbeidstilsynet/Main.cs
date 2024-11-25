using Altinn.Dan.Plugin.Arbeidstilsynet.Config;
using Altinn.Dan.Plugin.Arbeidstilsynet.Models;
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
        private readonly Settings _settings;
        private readonly IEvidenceSourceMetadata _metadata;
        private readonly IEntityRegistryService _entityRegistryService;

        private const int ERROR_NOT_FOUND = 1001;
        private const int ERROR_INVALID_ORG = 1002;
        private const int ERROR_OTHER = 1003;

        public Main(IHttpClientFactory httpClientFactory, IOptions<Settings> settings, IEvidenceSourceMetadata evidenceSourceMetadata, ILoggerFactory loggerFactory, IEntityRegistryService entityRegistryService)
        {
            _client = httpClientFactory.CreateClient("SafeHttpClient");
            _client.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", "nadobe/data.altinn.no"); // Source requires any user-agent to be set
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
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesBemanning(evidenceHarvesterRequest));
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesBemanning(EvidenceHarvesterRequest evidenceHarvesterRequest)
        {
            SimpleEntityRegistryUnit actualOrganization;

            if (evidenceHarvesterRequest.SubjectParty?.NorwegianOrganizationNumber is null)
            {
                throw new EvidenceSourcePermanentClientException(ERROR_INVALID_ORG,
                    "Expected a norwegian organization number");
            }

            if (_settings.IsTest)
            {
                actualOrganization = new SimpleEntityRegistryUnit { OrganizationNumber = evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber };
            }
            else
            {
                // The registry only has main units, so we have to traverse the enterprise structure in case the subject of the lookup is a subunit
                actualOrganization =
                    await _entityRegistryService.GetMainUnit(evidenceHarvesterRequest.SubjectParty
                        .NorwegianOrganizationNumber);
            }

            if (actualOrganization is null)
            {
                throw new EvidenceSourcePermanentClientException(ERROR_NOT_FOUND,
                    "Organization number not found in CCR");
            }

            var bemanning = await MakeRequest<Bemanning>(string.Format(_settings.BemanningUrl, actualOrganization.OrganizationNumber));

            var ecb = new EvidenceBuilder(_metadata, "Bemanningsforetakregisteret");
            ecb.AddEvidenceValue($"organisasjonsnummer", bemanning.Organisasjonsnummer, EvidenceSourceMetadata.SOURCE);
            ecb.AddEvidenceValue($"godkjenningsstatus", bemanning.Godkjenningsstatus, EvidenceSourceMetadata.SOURCE);

            return ecb.GetEvidenceValues();
        }

        [Function("Renholdsregisteret")]
        public async Task<HttpResponseData> Renhold(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestData req, FunctionContext context)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return  await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesRenhold(evidenceHarvesterRequest));
        }

        [Function("Bilpleieregisteret")]
        public async Task<HttpResponseData> Bilpleie(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestData req, FunctionContext context)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesBilpleie(evidenceHarvesterRequest));
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesBilpleie(EvidenceHarvesterRequest evidenceHarvesterRequest)
        {
            if (evidenceHarvesterRequest.SubjectParty?.NorwegianOrganizationNumber is null)
            {
                throw new EvidenceSourcePermanentClientException(ERROR_INVALID_ORG,
                    "Expected a norwegian organization number");
            }

            //The registry only has main units, so we have to traverse the enterprise structure in case the subject of the lookup is a subunit
            string actualOrgNo;

            if (_settings.IsTest)
            {
                //No tenor, no ER
                actualOrgNo = evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber;
            }
            else
            {
                var main = await _entityRegistryService.GetMainUnit(evidenceHarvesterRequest.SubjectParty
                    .NorwegianOrganizationNumber);

                if (main is null)
                {
                    throw new EvidenceSourcePermanentClientException(ERROR_NOT_FOUND,
                        "Organization number not found in CCR");
                }

                actualOrgNo = main.OrganizationNumber;
            }

            var result = await MakeRequest<BilpleieArbt>(string.Format(_settings.BilpleieURl, actualOrgNo));

            if (actualOrgNo != evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber) //subject is not main unit
            {
                var unit = result.data.underenheter.FirstOrDefault(x => x.organisasjonsnummer == evidenceHarvesterRequest.OrganizationNumber);
                if (unit != null)
                {
                    return CreateEvidenceResponse(unit.organisasjonsnummer, unit.registerstatus, unit.registerstatusTekst, unit.godkjenningsstatus);
                }
                throw new EvidenceSourcePermanentServerException(ERROR_NOT_FOUND, "Not found");
            }

            return CreateEvidenceResponse(result.data.organisasjonsnummer, result.data.registerstatus, result.data.registerstatusTekst, result.data.godkjenningsstatus);
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

            if (evidenceHarvesterRequest.SubjectParty?.NorwegianOrganizationNumber is null)
            {
                throw new EvidenceSourcePermanentClientException(ERROR_INVALID_ORG,
                    "Expected a norwegian organization number");
            }

            SimpleEntityRegistryUnit actualOrganization;

            if (_settings.IsTest)
            {
                actualOrganization = new SimpleEntityRegistryUnit { OrganizationNumber = evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber };
            }
            else
            {
                //The registry only has main units, so we have to traverse the enterprise structure in case the subject of the lookup is a subunit
                actualOrganization = await _entityRegistryService.GetMainUnit(evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber);

                if (actualOrganization is null)
                {
                    throw new EvidenceSourcePermanentClientException(ERROR_NOT_FOUND,
                        "Organization number not found in CCR");
                }
            }

            var content = await MakeRequest<Renhold>(string.Format(_settings.RenholdUrl, actualOrganization.OrganizationNumber));

            var ecb = new EvidenceBuilder(_metadata, "Renholdsregisteret");
            ecb.AddEvidenceValue($"organisasjonsnummer", content.Organisasjonsnummer, EvidenceSourceMetadata.SOURCE);
            ecb.AddEvidenceValue($"status", content.Status, EvidenceSourceMetadata.SOURCE);

           var statusChanged = Convert.ToDateTime(content.StatusEndret);

            if (statusChanged != DateTime.MinValue)
            {
                ecb.AddEvidenceValue($"statusEndret", statusChanged, EvidenceSourceMetadata.SOURCE, false);
            }

            return ecb.GetEvidenceValues();
        }

        private async Task<T> MakeRequest<T>(string url, string versionHeader = null) where T: new()
        {
            HttpResponseMessage result;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (versionHeader != null)
                    request.Headers.TryAddWithoutValidation("Content-Version", "1.1");

                result = await _client.SendAsync(request);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Target {url} exception: {ex}", url, ex.Message);
                throw new EvidenceSourceTransientException(ERROR_OTHER, ex.Message, ex);
            }

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Target {url} not found", url);
                throw new EvidenceSourcePermanentClientException(ERROR_NOT_FOUND, "Upstream returned 404");
            }

            if (!result.IsSuccessStatusCode)
            {
                _logger.LogInformation("Target {url} failed with status: {statusCode}", url, result.StatusCode);
                throw new EvidenceSourceTransientException(ERROR_OTHER, $"Upstream returned {result.StatusCode}");
            }

            var response = JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync());
            if (response == null)
            {
                throw new EvidenceSourceTransientException(ERROR_OTHER, "Failed to parse data model returned from upstream source");
            }

            return response;
        }

        [Function(Constants.EvidenceSourceMetadataFunctionName)]
        public async Task<HttpResponseData> Metadata(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req, FunctionContext context)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(_metadata.GetEvidenceCodes());
            return response;
        }
    }
}
