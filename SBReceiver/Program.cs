using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using SBShared.Models;

namespace SBReceiver
{
    class Program
    {
        private const string ConnectionString = "";
        private const string QueueName = "personqueue";

        private static IQueueClient _queueClient;

        static async Task Main(string[] args)
        {
            _queueClient = new QueueClient(ConnectionString, QueueName);
            
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

            Console.ReadLine();

            await _queueClient.CloseAsync();
        }

        private static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var jsonString = Encoding.UTF8.GetString(message.Body);
            var person = JsonSerializer.Deserialize<PersonModel>(jsonString);

            Console.WriteLine($"Person received: { person.FirstName } { person.LastName }");

            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine($"Message handle exception: { arg.Exception }");

            return Task.CompletedTask;
        }
    }
}
