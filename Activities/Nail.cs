using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Mjolnir
{
    public class Nail
    {
        private readonly IHttpClientFactory _factory;

        public Nail(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        [FunctionName(nameof(Nail))]
        public async Task<NailResult> Run([ActivityTrigger] NailActivityTrigger activityTrigger, ILogger log)
        {
            log.LogInformation($"Hammering {activityTrigger.TargetUrl} - Attempt Number: {activityTrigger.NailId}");

            var targetUri = new Uri(activityTrigger.TargetUrl);

            using (HttpClient client = _factory.CreateClient())
            {
                var message = new HttpRequestMessage(HttpMethod.Get, targetUri);
                message.Headers.Add("Cookie", activityTrigger.Cookies);

                Stopwatch watch = Stopwatch.StartNew();
                HttpResponseMessage resp = await client.SendAsync(message);
                watch.Stop();

                string content = string.Empty;
                if (!resp.IsSuccessStatusCode)
                {
                    log.LogError("Couldn't hammer url - {status}/{code} - {reason}. Duration: {duration}", resp.StatusCode, (int)resp.StatusCode, resp.ReasonPhrase, watch.Elapsed.TotalSeconds);
                }
                else
                {
                    log.LogInformation("Hammering url took {duration}", watch.Elapsed.TotalSeconds);

                    content = await resp.Content.ReadAsStringAsync();

                    log.LogDebug(content.Substring(0, 255));
                }

                resp.Headers.TryGetValues("X-Correlation-Id", out var correlationHeaders);
                correlationHeaders = correlationHeaders ?? new List<string>();

                return new NailResult()
                {
                    NailId = activityTrigger.NailId,
                    DurationMs = watch.Elapsed.TotalMilliseconds.ToString("##.000"),
                    StatusCode = resp.StatusCode,
                    Content = content.Substring(0, 255),
                    CorrelationId = string.Join('|', correlationHeaders),
                    ReasonPhrase = resp.ReasonPhrase
                };
            }
        }
    }
}