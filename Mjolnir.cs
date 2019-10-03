using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Mjolnir
{
	public class Nail
	{
		public string TargetUrl { get; set; }
		public string Cookies { get; set; }
		public int Attempts { get; set; }
	}

	public class NailActivity
	{
		public string TargetUrl { get; set; }
		public string Cookies { get; set; }
		public string NailId { get; set; }
	}

	public class Mjolnir
	{
		private readonly IHttpClientFactory _factory;

		public Mjolnir(IHttpClientFactory factory)
		{
			_factory = factory;
		}

		[FunctionName(nameof(Hammer))]
		public async Task<HttpResponseMessage> Hammer(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
			[OrchestrationClient]DurableOrchestrationClient durableClient,
			ILogger log
			)
		{
			string bodyJson = await req.Content.ReadAsStringAsync();
			var nail = JsonConvert.DeserializeObject<Nail>(bodyJson);
			
			string instanceId = await durableClient.StartNewAsync(nameof(HammerAllNailsAtOnce), nail);

			log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
			return await durableClient.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId);
		}

		[FunctionName(nameof(HammerAllNailsAtOnce))]
		public async Task HammerAllNailsAtOnce(
			[OrchestrationTrigger] DurableOrchestrationContext context)
		{
			var nail = context.GetInput<Nail>();
			var tasks = new List<Task>();

			for (var n = 0; n < nail.Attempts; n++)
			{
				Task t = context.CallActivityAsync(nameof(HammerNail), new NailActivity()
				{
					TargetUrl = nail.TargetUrl,
					Cookies = nail.Cookies,
					NailId = n.ToString()
				});

				tasks.Add(t);
			}

			await Task.WhenAll(tasks);
		}

		[FunctionName(nameof(HammerNail))]
		public async Task HammerNail([ActivityTrigger] NailActivity activity, ILogger log)
		{
			log.LogInformation($"Hammering {activity.TargetUrl} - Attempt Number: {activity.NailId}");
			
			var targetUri = new Uri(activity.TargetUrl);
			
			using (HttpClient client = _factory.CreateClient())
			{
				var message = new HttpRequestMessage(HttpMethod.Get, targetUri);
				message.Headers.Add("Cookie", activity.Cookies);

				Stopwatch watch = Stopwatch.StartNew();
				HttpResponseMessage resp = await client.SendAsync(message);
				watch.Stop();

				if (!resp.IsSuccessStatusCode)
				{
					log.LogError("Couldn't hammer url - {status}/{code} - {reason}. Duration: {duration}", resp.StatusCode, (int)resp.StatusCode, resp.ReasonPhrase, watch.Elapsed.TotalSeconds);
				}
				else
				{
					log.LogInformation("Hammering url took {duration}", watch.Elapsed.TotalSeconds);

					string content = await resp.Content.ReadAsStringAsync();

					log.LogDebug(content.Substring(0, 255));
				}
			}
		}
	}
}
