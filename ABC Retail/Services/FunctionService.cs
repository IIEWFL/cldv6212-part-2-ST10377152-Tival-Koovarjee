using ABC_Retail.Models;

namespace ABC_Retail.Services
{
    public class FunctionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _functionBaseUrl;

        public FunctionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _functionBaseUrl = configuration["AzureFunctionsBaseUrlProd"]
                               ?? throw new InvalidOperationException("Function base URL is missing");
        }

        public async Task<List<Customer>> GetCustomersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/customers");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Customer>>() ?? new List<Customer>();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not reach functions host at {_httpClient.BaseAddress}", ex);
            }
        }

        public async Task<Customer?> GetCustomerAsync(string partitionKey, string rowKey)
        {
            var response = await _httpClient.GetFromJsonAsync<Customer>($"{_functionBaseUrl}/api/customers/{partitionKey}/{rowKey}");
            return response;
        }

        public async Task<bool> AddCustomerAsync(Customer customer, IFormFile? photourl)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(customer.CustomerId ?? string.Empty), "CustomerId");
            content.Add(new StringContent(customer.FirstName ?? string.Empty), "FirstName");
            content.Add(new StringContent(customer.LastName ?? string.Empty), "LastName");
            content.Add(new StringContent(customer.Email ?? string.Empty), "Email");
            content.Add(new StringContent(customer.PhoneNumber ?? string.Empty), "PhoneNumber");

            if (photourl != null)
            {
                var streamContent = new StreamContent(photourl.OpenReadStream());
                content.Add(streamContent, "PhotoUrl", photourl.FileName);

            }

            var response = await _httpClient.PostAsync($"{_functionBaseUrl}/api/customers", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer, IFormFile? photourl)
        {
            var requestUrl = $"{_functionBaseUrl}/api/customers/{customer.PartitionKey}/{customer.RowKey}";
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(customer.CustomerId ?? string.Empty), "CustomerId");
            content.Add(new StringContent(customer.FirstName ?? string.Empty), "FirstName");
            content.Add(new StringContent(customer.LastName ?? string.Empty), "LastName");
            content.Add(new StringContent(customer.Email ?? string.Empty), "Email");
            content.Add(new StringContent(customer.PhoneNumber ?? string.Empty), "PhoneNumber");
            if (photourl != null)
            {
                var streamContent = new StreamContent(photourl.OpenReadStream());
                content.Add(streamContent, "PhotoUrl", photourl.FileName);
            }
            var response = await _httpClient.PostAsync($"{_functionBaseUrl}/api/customers/{customer.PartitionKey}/{customer.RowKey}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            var requestUrl = $"{_functionBaseUrl}/api/customers/{partitionKey}/{rowKey}";
            var response = await _httpClient.DeleteAsync(requestUrl);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<QueueLogViewModel>> GetMessagesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<QueueLogViewModel>>($"{_functionBaseUrl}/api/queuelogs");
            return response ?? new List<QueueLogViewModel>();
        }

        public async Task<string> ExportLog(string name)
        {
            var requestUrl = $"{_functionBaseUrl}/api/queuelogs?name={Uri.EscapeDataString(name)}";
            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
