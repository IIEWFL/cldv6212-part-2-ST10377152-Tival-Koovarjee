using ABC_Retail2.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

[assembly: FunctionsStartup(typeof(ABC_Retail_functions.StartUp))]

namespace ABC_Retail_functions
{
    public class StartUp : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // register table

            builder.Services.AddSingleton(sp =>
            CreateStorageService<CustomerService>(sp, "Customer", "table"));

            //register blob

            builder.Services.AddSingleton(sp =>
            CreateStorageService<BlobStorageService>(sp, "product-photos", "blob"));

            //register file share

            builder.Services.AddSingleton(sp =>
            CreateStorageService<FileShareStorageService>(sp, "retail-log-file", "fileShare"));

            //register queue

            builder.Services.AddSingleton(sp =>
            CreateStorageService<QueueStorageService>(sp, "orderqueue", "queue"));
        }

        private T CreateStorageService<T>(IServiceProvider sp, string serviceIdentifer, string serviceType) where T : class
        {
            var logger = sp.GetRequiredService<ILogger<StartUp>>();
            var configuration = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            if (string.IsNullOrEmpty(storageConnectionString) || string.IsNullOrWhiteSpace(serviceIdentifer))
            {
                logger.LogError("Storage connection string or service identifer not set");
                throw new InvalidOperationException("Configuration is invalid");
            }

            logger.LogInformation($"Using {serviceType} identifier: {serviceIdentifer}");

            return serviceType switch
            {
                "table" => new CustomerService(storageConnectionString, serviceIdentifer, sp.GetRequiredService<BlobStorageService>()) as T,
                "blob" => new BlobStorageService(storageConnectionString, serviceIdentifer) as T,
                "fileShare" => new FileShareStorageService(storageConnectionString, serviceIdentifer) as T,
                "queue" => new QueueStorageService(storageConnectionString, serviceIdentifer) as T,
                _ => throw new NotImplementedException($"{serviceType} is not supported")
            };
        }
    }
    }
