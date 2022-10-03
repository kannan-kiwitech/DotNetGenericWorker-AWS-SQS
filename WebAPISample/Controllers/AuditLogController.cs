using Microsoft.AspNetCore.Mvc;
using WebAPISample.Models;
using WebAPISample.Services.Contracts;

namespace WebAPISample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuditLogController : ControllerBase
    {
        private readonly ILogger<AuditLogController> _logger;
        private readonly IMessageService<AuditLogModel> _messageClient;

        public AuditLogController(ILogger<AuditLogController> logger, IMessageService<AuditLogModel> messageClient)
        {
            _logger = logger;
            _messageClient = messageClient;
        }

        [HttpPost]
        public async Task Post([FromBody] AuditLogModel model)
        {
            await _messageClient.SendMessage(model);
            _logger.LogInformation("Message pushed to the queue successfully.");
        }
    }
}