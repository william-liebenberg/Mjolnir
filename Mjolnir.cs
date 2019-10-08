using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Mjolnir
{
    public class Mjolnir
	{
		private readonly IHttpClientFactory _factory;

		public Mjolnir(IHttpClientFactory factory)
		{
			_factory = factory;
		}

        [FunctionName(nameof(GetTestOutput))]
        public async Task<IActionResult> GetTestOutput(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetTestOutput/{instanceId}")] HttpRequestMessage req,
            string instanceId,
            [OrchestrationClient]DurableOrchestrationClient durableClient,
            ILogger log
        )
        {
            log.LogInformation($"Getting test output for orchestration with ID = '{instanceId}'.");

            DurableOrchestrationStatus status = await durableClient.GetStatusAsync(instanceId, true);
            if (status?.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                var res = status.Output.ToObject<NailResult[]>();
                return new OkObjectResult(res);
            }

            return new OkObjectResult(status?.Output);
        }

        [FunctionName(nameof(ConcurrentHammer))]
		public async Task<HttpResponseMessage> ConcurrentHammer(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = "Hammer/AllAtOnce")] HttpRequestMessage req,
			[OrchestrationClient]DurableOrchestrationClient durableClient,
			ILogger log
			)
		{
			string bodyJson = await req.Content.ReadAsStringAsync();
			var hammerNailRequest = JsonConvert.DeserializeObject<HammerNailRequest>(bodyJson);
			
			string instanceId = await durableClient.StartNewAsync(nameof(HammerAllNailsAtOnce), hammerNailRequest);

			log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
			return durableClient.CreateCheckStatusResponse(req, instanceId);
		}

        [FunctionName(nameof(SingleHammer))]
        public async Task<HttpResponseMessage> SingleHammer(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Hammer/OneAtATime")] HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient durableClient,
            ILogger log
        )
        {
            string bodyJson = await req.Content.ReadAsStringAsync();
            var hammerNailRequest = JsonConvert.DeserializeObject<HammerNailRequest>(bodyJson);

            string instanceId = await durableClient.StartNewAsync(nameof(HammerAllNailsAtOnce), hammerNailRequest);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            return durableClient.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
