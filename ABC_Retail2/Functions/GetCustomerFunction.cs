//using ABC_Retail_functions.Models;
//using ABC_Retail_functions.Services;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Extensions.Logging;

//namespace ABC_Retail_functions.Functions;

//public class GetCustomerFunction
//{
//    private readonly CustomerService _customerService;

//    public GetCustomerFunction(CustomerService customerService)
//    {
//        _customerService = customerService;
//    }

//    [Function("GetCustomer")]
//    public async Task<IActionResult> Run(
//        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customer")] HttpRequest req, string partitionKey, string rowKey,
//        ILogger log)
//    {
//        log.LogInformation($"C# HTTP trigger function processed a request to get customer details based on PartitionKey{partitionKey} and rowKey{rowKey}");

//        var customer = await _customerService.GetCustomerAsync(partitionKey, rowKey);
//        if (customer == null)
//        {
//            log.LogWarning($"customer not found on partitionKey{partitionKey} and rowKey{rowKey}");
//            return new NotFoundResult();
//        }

//        var customerDto = new CustomerDto
//        {
//            PartitionKey = customer.PartitionKey,
//            RowKey = customer.RowKey,
//            Timestamp = customer.Timestamp,
//            ETag = customer.ETag.ToString(),
//            PhotoUrl = customer.PhotoUrl,
//            CustomerId = customer.CustomerId,
//            FirstName = customer.FirstName,
//            LastName = customer.LastName,
//            Email = customer.Email,
//            PhoneNumber = customer.PhoneNumber
//        };
//        return new OkObjectResult(customerDto);
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
    public class GetCustomerFunction
    {
        private readonly CustomerService _customerService;

        public GetCustomerFunction(CustomerService customerService)
        {
            _customerService = customerService;
        }

        [FunctionName("GetCustomer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey,
            string rowKey,
            ILogger log)
        {
            try
            {
                log.LogInformation($"Processing request to get customer details for PartitionKey={partitionKey}, RowKey={rowKey}");

                if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                {
                    return new BadRequestObjectResult("partitionKey and rowKey are required in the route. Use: GET /api/customers/{partitionKey}/{rowKey}");
                }

                var customer = await _customerService.GetCustomerAsync(partitionKey, rowKey);
                if (customer == null)
                {
                    log.LogWarning($"Customer not found for PartitionKey={partitionKey}, RowKey={rowKey}");
                    return new NotFoundResult();
                }

                var customerDto = new CustomerDto
                {
                    PartitionKey = customer.PartitionKey,
                    RowKey = customer.RowKey,
                    Timestamp = customer.Timestamp,
                    ETag = customer.ETag.ToString(),
                    PhotoUrl = customer.PhotoUrl,
                    CustomerId = customer.CustomerId,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber
                };

                return new OkObjectResult(customerDto);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error while retrieving customer");
                return new ObjectResult(new { error = ex.Message }) { StatusCode = 500 };
            }
        }
    }
}