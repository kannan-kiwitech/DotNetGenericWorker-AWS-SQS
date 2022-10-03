using WebAPISample.Models;
using WebAPISample.Services.Contracts;

namespace WebAPISample.Workers
{
    public class AuditLogWorker : BackgroundService
    {
        private readonly ILogger<AuditLogWorker> _logger;
        private readonly IMessageService<AuditLogModel> _messageClient;

        //Inject a service to implement the auditing logic
        //private readonly IAuditLogService _auditLogService;

        public AuditLogWorker(ILogger<AuditLogWorker> logger, IMessageService<AuditLogModel> messageClient
            //, IAuditLogService auditLogService
            )
        {
            _logger = logger;
            _messageClient = messageClient;
            //_auditLogService = auditLogService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("AuditLogWorker running at: {Time}", DateTime.Now);
                var messages = await _messageClient.ReceiveMessageAsync();

                foreach (var message in messages)
                {
                    //You can write your custom logic here...
                    //await _auditLogService.Log(message.Value);

                    _logger.LogInformation("AuditLogWorker processed message {userID}, {Action}", message.Value?.UserId, message.Value?.Message);

                    await _messageClient.DeleteMessageAsync(message.Key);
                }

                await Task.Delay(5000, stoppingToken); //Delay can be set according to your business requirement.
            }
        }
    }
}