using Altinn.Dan.Plugin.Arbeidstilsynet.Config;
using Altinn.Dan.Plugin.Arbeidstilsynet.Utils;
using Dan.Common.Exceptions;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dan.Common;

namespace Altinn.Dan.Plugin.Arbeidstilsynet
{
    public class Main
    {
        private ILogger _logger;
        private readonly HttpClient _client;
        private readonly IApplicationSettings _settings;
        private readonly IEvidenceSourceMetadata _metadata;

        public Main(IHttpClientFactory httpClientFactory, IApplicationSettings settings, IEvidenceSourceMetadata evidenceSourceMetadata)
        {
            _client = httpClientFactory.CreateClient("SafeHttpClient");
            _client.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", "nadobe/data.altinn.no");
            _settings = settings;
            _metadata = evidenceSourceMetadata;
        }

        [Function("Bemanningsforetakregisteret")]
        public async Task<HttpResponseData> Bemanning(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestData req, FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            _logger.LogInformation("Running func 'Bemanning'");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesBemanning(evidenceHarvesterRequest));
  
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesBemanning(EvidenceHarvesterRequest evidenceHarvesterRequest)
        {
            //The registry only has main units, so we have to traverse the enterprise structure in case the subject of the lookup is a subunit
            var actualOrganization = await BRUtils.GetMainUnit(evidenceHarvesterRequest.OrganizationNumber, _client);
            dynamic content = await MakeRequest(string.Format(_settings.BemanningUrl, actualOrganization.Organisasjonsnummer), actualOrganization.Organisasjonsnummer.ToString());

            var ecb = new EvidenceBuilder(_metadata, "Bemanningsforetakregisteret");
            ecb.AddEvidenceValue($"Organisasjonsnummer", content.Organisasjonsnummer, EvidenceSourceMetadata.SOURCE);
            ecb.AddEvidenceValue($"Godkjenningsstatus", content.Godkjenningsstatus, EvidenceSourceMetadata.SOURCE);

            return ecb.GetEvidenceValues();
        }

        [Function("Renholdsregisteret")]
        public async Task<HttpResponseData> Renhold(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestData req, FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            _logger.LogInformation("Running func 'Renhold'");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return  await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesRenhold(evidenceHarvesterRequest));
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesRenhold(EvidenceHarvesterRequest evidenceHarvesterRequest)
        {
            //The registry only has main units, so we have to traverse the enterprise structure in case the subject of the lookup is a subunit
            var actualOrganization = await BRUtils.GetMainUnit(evidenceHarvesterRequest.OrganizationNumber, _client);
            dynamic content = await MakeRequest(string.Format(_settings.RenholdUrl, actualOrganization.Organisasjonsnummer), actualOrganization.Organisasjonsnummer.ToString());

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

        private async Task<dynamic> MakeRequest(string target, string organizationNumber)
        {
            HttpResponseMessage result = null;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, target);
                result = await _client.SendAsync(request);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Target {target} exception: " + ex.Message);
                throw new EvidenceSourcePermanentServerException(EvidenceSourceMetadata.ERROR_CCR_UPSTREAM_ERROR, null, ex);
            }

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation($"Target {target} not found");
                throw new EvidenceSourcePermanentClientException(EvidenceSourceMetadata.ERROR_ORGANIZATION_NOT_FOUND, $"{organizationNumber} could not be found");
            }

            if (!result.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Target {target} failed with status: {result.StatusCode}" );
                throw new EvidenceSourceTransientException(EvidenceSourceMetadata.ERROR_CCR_UPSTREAM_ERROR, $"Request could not be processed");
            }

            var response = JsonConvert.DeserializeObject(await result.Content.ReadAsStringAsync());
            if (response == null)
            {
                throw new EvidenceSourcePermanentServerException(EvidenceSourceMetadata.ERROR_CCR_UPSTREAM_ERROR, "Did not understand the data model returned from upstream source");
            }

            return response;
        }

        [Function(Constants.EvidenceSourceMetadataFunctionName)]
        public async Task<HttpResponseData> Metadata(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequestData req, FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            _logger.LogInformation($"Running metadata for {Constants.EvidenceSourceMetadataFunctionName}");
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(_metadata.GetEvidenceCodes());
            return response;
        }
    }
}
