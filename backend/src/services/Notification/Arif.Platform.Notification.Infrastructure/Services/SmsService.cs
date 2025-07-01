using Microsoft.Extensions.Logging;
using Arif.Platform.Notification.Domain.Interfaces;
using Arif.Platform.Shared.Infrastructure.Services;

namespace Arif.Platform.Notification.Infrastructure.Services
{
    public class SmsService : ISmsService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<SmsService> _logger;

        public SmsService(
            ICurrentUserService currentUserService,
            ILogger<SmsService> logger)
        {
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<object> SendSmsAsync(object request)
        {
            _logger.LogInformation("Sending SMS");
            
            return new
            {
                SmsId = Guid.NewGuid().ToString(),
                Status = "Sent",
                SentAt = DateTime.UtcNow
            };
        }

        public async Task<object> GetSmsStatusAsync(Guid smsId)
        {
            _logger.LogInformation("Getting SMS status {SmsId}", smsId);
            
            return new
            {
                SmsId = smsId.ToString(),
                Status = "Delivered",
                DeliveredAt = DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<object>> GetSmsHistoryAsync()
        {
            _logger.LogInformation("Getting SMS history");
            
            return new List<object>
            {
                new { SmsId = Guid.NewGuid().ToString(), Status = "Delivered", SentAt = DateTime.UtcNow }
            };
        }

        public async Task<object> CreateSmsTemplateAsync(object request)
        {
            _logger.LogInformation("Creating SMS template");
            
            return new
            {
                TemplateId = Guid.NewGuid().ToString(),
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<object>> GetSmsTemplatesAsync()
        {
            _logger.LogInformation("Getting SMS templates");
            
            return new List<object>
            {
                new { TemplateId = Guid.NewGuid().ToString(), Name = "SMS Template", Type = "SMS" }
            };
        }
    }
}
