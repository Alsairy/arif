namespace Arif.Platform.ChatbotRuntime.Domain.Interfaces;

public interface IChannelService
{
    Task<IEnumerable<object>> GetChannelsAsync(Guid tenantId);
    Task<object> GetChannelAsync(Guid channelId, Guid tenantId);
    Task<object> CreateChannelAsync(object request);
    Task<object> UpdateChannelAsync(object request);
    Task<bool> DeleteChannelAsync(Guid channelId, Guid tenantId);
}
