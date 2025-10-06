//using ABC_Retail2.Models;
//using ABC_Retail2.Services;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using System.Threading.Tasks;

//namespace ABC_Retail2.Functions;

//public class UploadLogFileFunction
//{
//    private readonly QueueStorageService _queueStorageService;
//    private readonly FileShareStorageService _fileShareStorageService;

//    public UploadLogFileFunction(QueueStorageService queueStorageService, FileShareStorageService fileShareStorageService)
//    {
//        _queueStorageService = queueStorageService;
//        _fileShareStorageService = fileShareStorageService;
//    }

//    [Function("UploadLogFile")]
//    public async Task<IActionResult> Run(
//        [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "uploadlogfile")] HttpRequest req,
//        ILogger log)
//    {
//        log.LogInformation("C# HTTP trigger function processed a request to upload a log file.");

//        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
//        dynamic data = System.Text.Json.JsonSerializer.Deserialize<dynamic>(requestBody);
//        string name = data?.name;

//        if (string.IsNullOrEmpty(name))
//        {
//            return new BadRequestObjectResult("Please pass a valid file name in the request body");
//        }

//        try
//        {
//            List<QueueLogViewModel> logMessages = await _queueStorageService.GetMessagesAsync();

//            var content = new StringBuilder();
//            content.AppendLine("MessageId,InsertionTime,MessageText");

//            foreach (var logMessage in logMessages)
//            {
//                var messageText = logMessage.MessageText?.Replace("\"", "\"\""); // Escape quotes
//                content.AppendLine($"\"{logMessage.MessageId}\",\"{logMessage.InsertionTime.ToString("yyyy/MM/dd HH:mm:ss")}\",\"{messageText}\"");
//            }

//            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content.ToString())))
//            {
//                await _fileShareStorageService.UploadFileAsync(name, stream);
//            }

//            await _queueStorageService.ClearQueueAsync();

//            return new OkObjectResult($"Log file '{name}' uploaded successfully.");

//        }
//        catch (Exception ex)
//        {
//            log.LogError(ex, "Error uploading log file.");
//            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ABC_Retail2.Models;
using ABC_Retail2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace ABC_Retail2.Functions
{
    public class UploadLogFileFunction
    {
        private readonly QueueStorageService _queueStorageService;
        private readonly FileShareStorageService _fileShareStorageService;

        public UploadLogFileFunction(QueueStorageService queueStorageService, FileShareStorageService fileShareStorageService)
        {
            _queueStorageService = queueStorageService;
            _fileShareStorageService = fileShareStorageService;
        }

        [FunctionName("UploadLogFile")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "uploadlogfile")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Processing request to upload a log file.");

            try
            {
                using var reader = new StreamReader(req.Body);
                var requestBody = await reader.ReadToEndAsync();
                var jsonDoc = System.Text.Json.JsonDocument.Parse(string.IsNullOrWhiteSpace(requestBody) ? "{}" : requestBody);
                string name = null;
                if (jsonDoc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object &&
                    jsonDoc.RootElement.TryGetProperty("name", out var nameProp))
                {
                    name = nameProp.GetString();
                }

                if (string.IsNullOrEmpty(name))
                    return new BadRequestObjectResult("Please pass a valid file name in the request body (JSON: { \"name\": \"fileName.csv\" })");

                List<QueueLogViewModel> logMessages = await _queueStorageService.GetMessagesAsync();

                var content = new StringBuilder();
                content.AppendLine("MessageId,InsertionTime,MessageText");

                foreach (var logMessage in logMessages)
                {
                    var messageText = logMessage.MessageText?.Replace("\"", "\"\""); // Escape quotes
                    content.AppendLine($"\"{logMessage.MessageId}\",\"{logMessage.InsertionTime:yyyy/MM/dd HH:mm:ss}\",\"{messageText}\"");
                }

                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content.ToString()));
                await _fileShareStorageService.UploadFileAsync(name, stream);

                await _queueStorageService.ClearQueueAsync();

                return new OkObjectResult($"Log file '{name}' uploaded successfully.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error uploading log file.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
