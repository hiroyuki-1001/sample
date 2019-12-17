using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApp7
{
    public static class Function1
    {
        [FunctionName(nameof(TopOrchestrator))]
        public static async Task TopOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context, ILogger log)
        {
            if (!context.IsReplaying) log.LogInformation($"{nameof(TopOrchestrator)} - Start. InstanceId={context.InstanceId}");

            for (int i = 0; i < 100; i++)
            {
                await context.CallSubOrchestratorAsync<string>(nameof(SubOrchestrator), i);
            }

            if (!context.IsReplaying) log.LogInformation($"{nameof(TopOrchestrator)} - End. InstanceId={context.InstanceId}");
        }

        [FunctionName(nameof(SubOrchestrator))]
        public static async Task SubOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var i = context.GetInput<int>();
            await context.CallActivityAsync<string>(nameof(SayHello), i.ToString());
        }

        [FunctionName(nameof(SayHello))]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName(nameof(HttpStart))]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync(nameof(TopOrchestrator), null);
            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}