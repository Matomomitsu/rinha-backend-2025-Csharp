using RinhaBackend.Models;

namespace RinhaBackend.Services
{
    public interface IPaymentQueue
    {
        ValueTask QueuePaymentAsync(PaymentSummaryData payment);
    }
}