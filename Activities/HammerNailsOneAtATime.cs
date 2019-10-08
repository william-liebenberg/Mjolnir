using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace Mjolnir
{
    public class HammerNailsOneAtATime
    {
        [FunctionName(nameof(HammerNailsOneAtATime))]
        public async Task<List<NailResult>> Run(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var nail = context.GetInput<HammerNailRequest>();
            var results = new List<NailResult>();

            for (var n = 0; n < nail.Attempts; n++)
            {
                NailResult t = await context.CallActivityAsync<NailResult>(nameof(Nail), new NailActivityTrigger()
                {
                    TargetUrl = nail.TargetUrl,
                    Cookies = nail.Cookies,
                    NailId = n.ToString()
                });

                results.Add(t);
            }

            return results;
        }
    }
}
