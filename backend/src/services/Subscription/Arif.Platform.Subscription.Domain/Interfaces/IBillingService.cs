namespace Arif.Platform.Subscription.Domain.Interfaces;

public interface IBillingService
{
    Task<object> ProcessPaymentAsync(object request);
    Task<IEnumerable<object>> GetInvoicesAsync(object request);
    Task<object> GetInvoiceAsync(Guid invoiceId, Guid tenantId);
    Task<object> CreateInvoiceAsync(object request);
    Task<object> AddPaymentMethodAsync(object request);
    Task<object> GetPaymentMethodsAsync(Guid tenantId);
    Task<bool> RemovePaymentMethodAsync(Guid paymentMethodId, Guid tenantId);
    Task<object> RefundPaymentAsync(Guid paymentId);
}
