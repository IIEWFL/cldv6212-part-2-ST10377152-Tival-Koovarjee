using ABC_Retail2.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ABC_Retail2.Services
{
    public class QueueStorageService
    {
        private readonly QueueClient _queueClient;

        public QueueStorageService(string storageConnectionString, string queueName)
        {
            var queueServiceClient = new QueueServiceClient(storageConnectionString);
            _queueClient = queueServiceClient.GetQueueClient(queueName);
            _queueClient.CreateIfNotExists();
        }

        //send message to queue
        public async Task SendMessagesAsync(object message)
        {
            //convert message to object JSON string
            var messageJson = JsonSerializer.Serialize(message);
            await _queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(messageJson)));
        }
        //get messages from queue for the log

        public async Task<List<QueueLogViewModel>> GetMessagesAsync(int maxMessages = 10)
        {
            var messages = await _queueClient.PeekMessagesAsync(maxMessages);
            var messageList = new List<QueueLogViewModel>();
            foreach (PeekedMessage message in messages.Value)
            {
                var messageText = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                var logEntry = new QueueLogViewModel
                {
                    MessageId = message.MessageId,
                    InsertionTime = message.InsertedOn ?? DateTimeOffset.MinValue,
                    MessageText = messageText
                };
                messageList.Add(logEntry);
            }
            return messageList;
        }

        public async Task ClearQueueAsync()
        {
            await _queueClient.ClearMessagesAsync();
        }

    }
}
