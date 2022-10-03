using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using WebAPISample.Services.Contracts;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace WebAPISample.Services.Implementations
{
    public class SqsGenericService<T> : IMessageService<T>
    {
        private readonly ILogger<SqsGenericService<T>> _logger;
        private readonly IAmazonSQS _amazonSQSClient;
        private readonly string _queueUrl;

        public SqsGenericService(ILogger<SqsGenericService<T>> logger, IConfiguration configuration, IHostingEnvironment env)
        {
            _logger = logger;
            var options = configuration.GetAWSOptions();
            //This queueName will be used to create the SQS Queue for each type of object in different environments
            var queueName = $"que-{env.EnvironmentName.ToLower()}-{typeof(T).Name.ToLower()}";
            _amazonSQSClient = options.CreateServiceClient<IAmazonSQS>();
            _queueUrl = GetQueueUrl(queueName).Result;
        }

        public async Task<Dictionary<string, T?>> ReceiveMessageAsync(int maxMessages = 1)
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = maxMessages
            };

            var messages = await _amazonSQSClient.ReceiveMessageAsync(request);

            _logger.LogInformation("{number} Message(s) received from {queue}", messages.Messages.Count, _queueUrl);
            return messages.Messages.ToDictionary(a => a.ReceiptHandle, a => JsonConvert.DeserializeObject<T>(a.Body));
        }

        public async Task DeleteMessageAsync(string id)
        {
            var request = new DeleteMessageRequest
            {
                QueueUrl = _queueUrl,
                ReceiptHandle = id
            };

            await _amazonSQSClient.DeleteMessageAsync(request);
            _logger.LogInformation("Message {id} deleted successfully!", id);
        }

        private async Task<string> GetQueueUrl(string queueName)
        {
            try
            {
                var response = await _amazonSQSClient.GetQueueUrlAsync(new GetQueueUrlRequest
                {
                    QueueName = queueName
                });
                _logger.LogInformation("Queue {queueName} already exists.", queueName);
                return response.QueueUrl;
            }
            catch (QueueDoesNotExistException)
            {
                //You might want to add additionale exception handling here because that may fail
                _logger.LogWarning("No queue {queueName} exists, creating new...", queueName);
                var response = await _amazonSQSClient.CreateQueueAsync(new CreateQueueRequest
                {
                    QueueName = queueName
                });
                return response.QueueUrl;
            }
        }

        public async Task SendMessage(T message)
        {
            var messageBody = JsonConvert.SerializeObject(message);
            await _amazonSQSClient.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = messageBody
            });

            _logger.LogInformation("Message {message} send successfully to {_queueUrl}.", message, _queueUrl);
        }
    }
}