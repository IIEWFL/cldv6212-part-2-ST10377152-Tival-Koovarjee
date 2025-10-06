//using ABC_Retail2.Services;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.Extensions.Logging;
//using System.Threading.Tasks;

//namespace ABC_Retail.Functions;

//public class GetmessagesFunction
//{
//    private readonly QueueStorageService _queueStorageService;

//    public GetmessagesFunction(QueueStorageService queueStorageService)
//    {
//        _queueStorageService = queueStorageService;
//    }

//    [Function("GetMessages")]

//    public async Task<IActionResult> Run(
//        [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "customerlogs")] HttpRequest req,
//        ILogger log)

//    {
//        log.LogInformation("C# HTTP trigger function processed a request to get customer logs.");

//        var messages = await _queueStorageService.GetMessagesAsync();
//        return new OkObjectResult(messages);
//    }
//}

using System.Threading.Tasks;
using ABC_Retail2.Services;           // ensure the namespace matches your project
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace ABC_Retail2.Functions        // keep this consistent across your project
{
    public class GetMessagesFunction
    {
        private readonly QueueStorageService _queueStorageService;

        public GetMessagesFunction(QueueStorageService queueStorageService)
        {
            _queueStorageService = queueStorageService;
        }

        [FunctionName("GetMessages")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customerlogs")] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Processing request to get customer logs.");

                var messages = await _queueStorageService.GetMessagesAsync();
                return new OkObjectResult(messages);
            }
            catch (System.Exception ex)
            {
                log.LogError(ex, "Error retrieving messages from queue storage.");
                return new ObjectResult(new { error = ex.Message }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }
    }
}
