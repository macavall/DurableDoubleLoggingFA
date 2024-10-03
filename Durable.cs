using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoubleLoggingDurable
{
    public static class Durable
    {
        [Function(nameof(Durable))]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(Durable));
            logger.LogInformation("Saying hello.");
            var outputs = new List<string>();

            logger.LogInformation("Calling Tasks / Activities");

            // Replace name and input with values relevant for your Durable Functions Activity
            logger.LogInformation("Calling Tokyo");
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));
            logger.LogInformation("Calling Seattle");
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
            logger.LogInformation("Calling London");
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));

            logger.LogInformation("All Tasks completed");

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [Function(nameof(SayHello))]
        public static string SayHello([ActivityTrigger] string name, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("SayHello");
            logger.LogInformation("Saying hello to {name}.", name);
            return $"Hello {name}!";
        }

        [Function("Durable_HttpStart")]
        public static async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("Durable_HttpStart");

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(Durable));

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            // Returns an HTTP 202 response with an instance management payload.
            // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
            return await client.CreateCheckStatusResponseAsync(req, instanceId);
        }
    }
}
