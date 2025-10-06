//using ABC_Retail2.Services;
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

//public class UpdateCustomerFunction
//{
//    private readonly CustomerService _customerService;
//    private readonly BlobStorageService _blobStorageService;
//    private readonly QueueStorageService _queueStorageService;

//    public UpdateCustomerFunction(CustomerService customerService, BlobStorageService blobStorageService, QueueStorageService queueStorageService)
//    {
//        _customerService = customerService;
//        _blobStorageService = blobStorageService;
//        _queueStorageService = queueStorageService;
//    }

//    [Function("UpdateCustomer")]

//    public async Task<IActionResult> Run(
//         [HttpTrigger(AuthorizationLevel.Anonymous, "Put", Route = "customers/{partitionKey}/{rowKey}")] HttpRequest req, string partitionKey, string rowKey,
//         ILogger log)
//    {
//        log.LogInformation("C# HTTP trigger function processed a request to update a customer.");
//        try
//        {
//            var form = await req.ReadFormAsync();
//            var name = form["name"].FirstOrDefault();
//            var email = form["email"].FirstOrDefault();
//            var phone = form["phone"].FirstOrDefault();
//            var imageFile = form.Files.GetFile("photo");
//            var customer = await _customerService.GetCustomerAsync(partitionKey, rowKey);
//            if (customer == null)
//            {
//                log.LogWarning($"Customer not found with PartitionKey: {partitionKey}, RowKey: {rowKey}");
//                return new NotFoundResult();
//            }
//            if (!string.IsNullOrEmpty(name)) customer.FirstName = name;
//            if (!string.IsNullOrEmpty(email)) customer.Email = email;
//            if (!string.IsNullOrEmpty(phone)) customer.PhoneNumber = phone;
//            if (imageFile != null && imageFile.Length > 0)
//            {
//                using var stream = imageFile.OpenReadStream();
//                var blobName = $"{Guid.NewGuid()}_{imageFile.FileName}";
//                await _customerService.UpdateCustomerAsync(customer, stream, blobName);
//            }
//            else
//            {
//                await _customerService.UpdateCustomerAsync(customer);
//            }
//            await _queueStorageService.SendMessagesAsync(new { Action = "Update", Customer = customer });
//            return new OkObjectResult(customer);
//        }
//        catch (Exception ex)
//        {
//            log.LogError(ex, $"Error updating customer: PartitionKey: {partitionKey}, RowKey: {rowKey}");
//            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
//        }
//    }
//}

using System;
using System.Linq;
using System.Threading.Tasks;
using ABC_Retail2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace ABC_Retail2.Functions
{
    public class UpdateCustomerFunction
    {
        private readonly CustomerService _customerService;
        private readonly BlobStorageService _blobStorageService;
        private readonly QueueStorageService _queueStorageService;

        public UpdateCustomerFunction(
            CustomerService customerService,
            BlobStorageService blobStorageService,
            QueueStorageService queueStorageService)
        {
            _customerService = customerService;
            _blobStorageService = blobStorageService; // <- make sure your field name matches exactly (_blobStorageService)
            _queueStorageService = queueStorageService; // <- same note for _queueStorageService
        }

        [FunctionName("UpdateCustomer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "customers/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey,
            string rowKey,
            ILogger log)
        {
            log.LogInformation($"Processing request to update customer. PartitionKey={partitionKey}, RowKey={rowKey}");

            try
            {
                var form = await req.ReadFormAsync();

                var name = form["name"].FirstOrDefault();
                var email = form["email"].FirstOrDefault();
                var phone = form["phone"].FirstOrDefault();
                var imageFile = form.Files.FirstOrDefault(); // reads first file in form-data (key can be anything)

                var customer = await _customerService.GetCustomerAsync(partitionKey, rowKey);
                if (customer == null)
                {
                    log.LogWarning($"Customer not found: {partitionKey}/{rowKey}");
                    return new NotFoundResult();
                }

                if (!string.IsNullOrEmpty(name)) customer.FirstName = name;
                if (!string.IsNullOrEmpty(email)) customer.Email = email;
                if (!string.IsNullOrEmpty(phone)) customer.PhoneNumber = phone;

                if (imageFile != null && imageFile.Length > 0)
                {
                    using var stream = imageFile.OpenReadStream();
                    var blobName = $"{Guid.NewGuid()}_{imageFile.FileName}";
                    // If your UpdateCustomerAsync has an overload for stream + blobName:
                    await _customerService.UpdateCustomerAsync(customer, stream, blobName);
                }
                else
                {
                    await _customerService.UpdateCustomerAsync(customer);
                }

                await _queueStorageService.SendMessagesAsync(new { Action = "Update", Customer = customer });

                return new OkObjectResult(customer);
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Error updating customer: {partitionKey}/{rowKey}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
