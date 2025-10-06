#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC_Retail2.Models
{
    public class CustomerDto
    {
        public string? PartitionKey { get; set; }

        public string? RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public string? ETag { get; set; }

        public string? PhotoUrl { get; set; }

        public string? CustomerId { get; set; }
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }


    }
}
