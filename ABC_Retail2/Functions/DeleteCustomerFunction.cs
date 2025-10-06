//using ABC_Retail2.Services;
//using Azure;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ABC_Retail2.Functions;

//public class DeleteCustomerFunction
//{
//    private readonly CustomerService _customerService;
//    private readonly BlobStorageService _blobStorageService;
//    private readonly QueueStorageService _queueStorageService;

//    public DeleteCustomerFunction(CustomerService customerService, BlobStorageService blobStorageService, QueueStorageService queueStorageService)
//    {
//        _customerService = customerService;
//        _blobStorageService = blobStorageService;
//        _queueStorageService = queueStorageService;
//    }

//    [Function("DeleteCustomer")]
//    public async Task<IActionResult> Run(
//        [HttpTrigger(AuthorizationLevel.Anonymous, "Delete", Route = "cutomers/{partitionKey}/{rowKey}")] HttpRequest req, string partitionKey, string rowKey,
//        ILogger log)

//    {
//        log.LogInformation("C# HTTP trigger function processed a request to delete a customer.");

//        try
//        {
//            var customer = await _customerService.GetCustomerAsync(partitionKey, rowKey);
//            if (customer == null)
//            {
//                log.LogWarning($"customer not found on partitionKey{partitionKey} and rowKey{rowKey}");
//                return new NotFoundResult();
//            }
//            await _customerService.DeleteCustomerAsync(partitionKey, rowKey);

//            if (!string.IsNullOrEmpty(customer.PhotoUrl))
//            {
//                var blobName = new Uri(customer.PhotoUrl).Segments.Last();
//                await _blobStorageService.DeletePhotoAsync(customer.PhotoUrl);
//            }

//            await _queueStorageService.SendMessagesAsync(new { Action = "Delete", Customer = customer });
//            return new OkResult();
//        }
//        catch (RequestFailedException ex) when (ex.Status == 404)
//        {
//            log.LogWarning($"customer not found on partitionKey{partitionKey} and rowKey{rowKey}");
//            return new NotFoundResult();
//        }
//        catch (Exception ex)
//        {
//            log.LogError(ex, $"Error deleting customer: PartitionKey: {partitionKey}, RowKey: {rowKey}");
//            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
//        }

//    }
//}

using System;
using System.Linq;
using System.Threading.Tasks;
using ABC_Retail2.Services;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace ABC_Retail2.Functions
{
    public class DeleteCustomerFunction
    {
        private readonly CustomerService _customerService;
        private readonly BlobStorageService _blobStorageService;
        private readonly QueueStorageService _queueStorageService;

        public DeleteCustomerFunction(
            CustomerService customerService,
            BlobStorageService blobStorageService,
            QueueStorageService queueStorageService)
        {
            _customerService = customerService;
            _blobStorageService = blobStorageService;
            _queueStorageService = queueStorageService; // <-- fix if your DI name differs
        }

        [FunctionName("DeleteCustomer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "customers/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey,
            string rowKey,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger processed a request to delete customer. PartitionKey={partitionKey}, RowKey={rowKey}");

            try
            {
                var customer = await _customerService.GetCustomerAsync(partitionKey, rowKey);
                if (customer == null)
                {
                    log.LogWarning($"Customer not found for PartitionKey={partitionKey}, RowKey={rowKey}");
                    return new NotFoundResult();
                }

                await _customerService.DeleteCustomerAsync(partitionKey, rowKey);

                if (!string.IsNullOrEmpty(customer.PhotoUrl))
                {
                    // If your DeletePhotoAsync expects blob name, extract it; otherwise pass URL as required.
                    var blobName = new Uri(customer.PhotoUrl).Segments.Last();
                    await _blobStorageService.DeletePhotoAsync(blobName); // use blobName if method expects name
                }

                await _queueStorageService.SendMessagesAsync(new { Action = "Delete", Customer = customer });
                return new OkResult();
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                log.LogWarning(ex, $"Azure resource not found while deleting customer: {partitionKey}/{rowKey}");
                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Error deleting customer: PartitionKey={partitionKey}, RowKey={rowKey}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}