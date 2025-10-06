//using ABC_Retail_functions.Models;
//using ABC_Retail_functions.Services;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Extensions.Logging;

//namespace ABC_Retail_functions.Functions;

//public class CreateCustomerFunction
//{
//    private readonly CustomerService _customerService;
//    private readonly BlobStorageService _blobStorageService;
//    private readonly QueueStorageService _queueStorageService;

//    public CreateCustomerFunction(CustomerService customerService, BlobStorageService blobStorageService, QueueStorageService queueStorageService)
//    {
//        _customerService = customerService;
//        _blobStorageService = blobStorageService;
//        _queueStorageService = queueStorageService;
//    }

//    [Function("CreateCustomer")]

//    public async Task<IActionResult> Run(
//        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customers")] HttpRequest req,
//        ILogger log)

//    {
//        log.LogInformation("C# HTTP trigger function processed a request to create a customer.");

//        var form = await req.ReadFormAsync();
//        var partitionKey = "customers";
//        var rowKey = Guid.NewGuid().ToString();
//        var customer = new Customer
//        {
//            PartitionKey = partitionKey,
//            RowKey = rowKey,
//            FirstName = form["FirstName"],
//            LastName = form["LastName"],
//            Email = form["Email"],
//            PhoneNumber = form["PhoneNumber"],
//            CustomerId = rowKey
//        };

//        log.LogInformation($"PartitionKey: {customer.PartitionKey}, RowKey: {customer.RowKey}");

//        if (req.Form.Files.Count > 0)
//        {
//             var photo = req.Form.Files[0];
//            using var stream = photo.OpenReadStream();
//            customer.PhotoUrl = await _blobStorageService.UploadPhotoAsync(Guid.NewGuid().ToString(), stream);

//        }

//        if (string.IsNullOrEmpty(customer.PartitionKey) || string.IsNullOrEmpty(customer.RowKey))
//        {
//            return new BadRequestObjectResult("PartitionKey and RowKey are required.");
//        }

//        await _customerService.AddCustomerAsync(customer);

//        await _queueStorageService.SendMessagesAsync(new { Action = "Create", Customer = customer });

//        return new OkObjectResult(customer);
//    }
//}


using ABC_Retail2.Models;
using ABC_Retail2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ABC_Retail2.Functions
{
    public class CreateCustomerFunction
    {
        private readonly CustomerService _customerService;
        private readonly BlobStorageService _blobStorageService;
        private readonly QueueStorageService _queueStorageService;

        public CreateCustomerFunction(
            CustomerService customerService,
            BlobStorageService blobStorageService,
            QueueStorageService queueStorageService)
        {
            _customerService = customerService;
            _blobStorageService = blobStorageService;
            _queueStorageService = queueStorageService;
        }

        [FunctionName("CreateCustomer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customers")] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request to create a customer.");

                var form = await req.ReadFormAsync();
                var partitionKey = "customers";
                var rowKey = Guid.NewGuid().ToString();
                var customer = new Customer
                {
                    PartitionKey = partitionKey,
                    RowKey = rowKey,
                    FirstName = form["FirstName"],
                    LastName = form["LastName"],
                    Email = form["Email"],
                    PhoneNumber = form["PhoneNumber"],
                    CustomerId = rowKey
                };

                if (req.Form.Files.Count > 0)
                {
                    var photo = req.Form.Files[0];
                    using var stream = photo.OpenReadStream();
                    customer.PhotoUrl = await _blobStorageService.UploadPhotoAsync(Guid.NewGuid().ToString(), stream);
                }

                if (string.IsNullOrEmpty(customer.PartitionKey) || string.IsNullOrEmpty(customer.RowKey))
                    return new BadRequestObjectResult("PartitionKey and RowKey are required.");

                await _customerService.AddCustomerAsync(customer);
                await _queueStorageService.SendMessagesAsync(new { Action = "Create", Customer = customer });

                return new OkObjectResult(customer);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error creating customer");
                // Return the message so you can see it in Postman while debugging (remove in production)
                return new ObjectResult(new { error = ex.Message }) { StatusCode = 500 };
            }
        }
    }
}