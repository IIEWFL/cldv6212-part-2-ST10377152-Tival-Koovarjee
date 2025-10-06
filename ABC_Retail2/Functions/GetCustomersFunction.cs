//using ABC_Retail2.Models;
//using ABC_Retail2.Services;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.Extensions.Logging;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ABC_Retail2.Functions;

//public class GetCustomersFunction
//{
//    private readonly CustomerService _customerService;

//    public GetCustomersFunction(CustomerService customerService)
//    {
//        _customerService = customerService;
//    }

//    [Function("GetCustomers")]
//    public async Task<IActionResult> Run(
//        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers")] HttpRequest req,
//        ILogger log)
//    {
//        {
//            log.LogInformation("C# HTTP trigger function processed a request.");


//            var customers = await _customerService.GetCustomersAsync();


//            var customerDtos = customers.Select(c => new CustomerDto
//            {
//                PartitionKey = c.PartitionKey,
//                RowKey = c.RowKey,
//                Timestamp = c.Timestamp,
//                ETag = c.ETag.ToString(),
//                PhotoUrl = c.PhotoUrl,
//                CustomerId = c.CustomerId,
//                FirstName = c.FirstName,
//                LastName = c.LastName,
//                Email = c.Email,
//                PhoneNumber = c.PhoneNumber
//            }).ToList();


//            return new OkObjectResult(customerDtos);
//        }
//    }
//}

using System.Linq;
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
    public class GetCustomersFunction
    {
        private readonly CustomerService _customerService;

        public GetCustomersFunction(CustomerService customerService)
        {
            _customerService = customerService;
        }

        [FunctionName("GetCustomers")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers")] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Processing request to get all customers.");

                var customers = await _customerService.GetCustomersAsync();

                var customerDtos = customers.Select(c => new CustomerDto
                {
                    PartitionKey = c.PartitionKey,
                    RowKey = c.RowKey,
                    Timestamp = c.Timestamp,
                    ETag = c.ETag.ToString(),
                    PhotoUrl = c.PhotoUrl,
                    CustomerId = c.CustomerId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber
                }).ToList();

                return new OkObjectResult(customerDtos);
            }
            catch (System.Exception ex)
            {
                log.LogError(ex, "Error while retrieving customers.");
                return new ObjectResult(new { error = ex.Message }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }
    }
}