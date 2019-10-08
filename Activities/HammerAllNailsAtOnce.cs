using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace Mjolnir
{
    public class HammerAllNailsAtOnce
    {
        [FunctionName(nameof(HammerAllNailsAtOnce))]
        public async Task<List<NailResult>> Run(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var nail = context.GetInput<HammerNailRequest>();
            var tasks = new List<Task<NailResult>>();

            for (var n = 0; n < nail.Attempts; n++)
            {
                Task<NailResult> t = context.CallActivityAsync<NailResult>(nameof(Nail), new NailActivityTrigger()
                {
                    TargetUrl = nail.TargetUrl,
                    Cookies = nail.Cookies,
                    NailId = n.ToString()
                });

                tasks.Add(t);
            }

            return (await Task.WhenAll(tasks)).ToList();
        }
    }
}
