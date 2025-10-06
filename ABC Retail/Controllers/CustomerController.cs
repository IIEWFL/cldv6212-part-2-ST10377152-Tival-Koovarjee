using ABC_Retail.Services;
using ABC_Retail.Services.Storage;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ABC_Retail.Models;
using System.Text;

namespace ABC_Retail.Controllers
{
    public class CustomerController : Controller
    {
        private readonly FunctionService _functionService;
        private readonly CustomerService _customerService;
        private readonly BlobStorageService _blobStorageService;
        private readonly QueueStorageService _queueStorageService;
        private readonly FileShareStorageService fileShareStorageService;

        public CustomerController(FunctionService functionService, Services.CustomerService customerService, BlobStorageService blobStorageService, QueueStorageService queueStorageService, FileShareStorageService fileShareStorageService)
        {
            _functionService = functionService;
            _customerService = customerService;
            _blobStorageService = blobStorageService;
            _queueStorageService = queueStorageService;
            this.fileShareStorageService = fileShareStorageService;
        }

        // GET: Customer/Index
        public async Task<IActionResult> Index()
        {
            var customers = await _functionService.GetCustomersAsync();
            return View(customers);
        }

        // GET: /Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer, IFormFile? image) 
        {
            if (ModelState.IsValid) 
            {
                await _functionService.AddCustomerAsync(customer, image);
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public  async Task<IActionResult > Details(string partitionKey, string rowKey)
        {
            var customer = await _functionService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        //Get Customer/Edit
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var customer = await _functionService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        //Post Customer/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer, IFormFile? newImage)
        {
           if (ModelState.IsValid)
            {
                await _functionService.UpdateCustomerAsync(customer, newImage);
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        //Get Customer/Delete
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var customer = await _functionService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        //Post Customer/Delete

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
           await _functionService.DeleteCustomerAsync(partitionKey, rowKey);
              return RedirectToAction(nameof(Index));
        }

        //get customerLogs/log
        [HttpGet]
        public async Task<IActionResult> Log() 
        {
            var logMessages = await _functionService.GetMessagesAsync();
            return View(logMessages);
        }

        //post exportlog
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportLog() 
        {
            var filename = $"CustomerLog_{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}.csv";
            var responseMessage = await _functionService.ExportLog(filename);
            return RedirectToAction(nameof(Index));
        }
    }
}
