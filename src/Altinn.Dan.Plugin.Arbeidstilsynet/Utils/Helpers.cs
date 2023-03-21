using Dan.Common.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Altinn.Dan.Plugin.Arbeidstilsynet.Utils
{
    public static class Helpers
    {

        public static async Task<T> MakeRequest<T>(HttpClient client, string target, string organizationNumber, ILogger logger, string versionHeader = null) where T: new()
        {
            HttpResponseMessage result = null;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, target);

                if (versionHeader != null)
                    request.Headers.TryAddWithoutValidation("Content-Version", "1.1");

                result = await client.SendAsync(request);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError($"Target {target} exception: " + ex.Message);
                throw new EvidenceSourcePermanentServerException(EvidenceSourceMetadata.ERROR_CCR_UPSTREAM_ERROR, null, ex);
            }

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogInformation($"Target {target} not found");
                throw new EvidenceSourcePermanentClientException(EvidenceSourceMetadata.ERROR_ORGANIZATION_NOT_FOUND, $"{organizationNumber} could not be found");
            }

            if (!result.IsSuccessStatusCode)
            {
                logger.LogInformation($"Target {target} failed with status: {result.StatusCode}");
                throw new EvidenceSourceTransientException(EvidenceSourceMetadata.ERROR_CCR_UPSTREAM_ERROR, $"Request could not be processed");
            }

            var response = JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync());
            if (response == null)
            {
                throw new EvidenceSourcePermanentServerException(EvidenceSourceMetadata.ERROR_CCR_UPSTREAM_ERROR, "Did not understand the data model returned from upstream source");
            }

            return response;
        }
    }
}
