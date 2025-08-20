using RinhaBackend.Models;
using System.Net;

namespace RinhaBackend.Services
{
    public class PaymentsServices
    {
        List<PaymentSummaryData> paymentSummaryDataDefault = new List<PaymentSummaryData>();
        List<PaymentSummaryData> paymentSummaryDataFallback = new List<PaymentSummaryData>();
        List<PaymentSummaryData> errorPaymentsList = new List<PaymentSummaryData>();
        List<PaymentSummaryData> paymentsList = new List<PaymentSummaryData>();
        
        private IPaymentQueue? _paymentQueue;

        public PaymentsServices()
        {
            // Constructor without dependencies to break circular dependency
        }

        // Method to set the payment queue after service creation
        public void SetPaymentQueue(IPaymentQueue paymentQueue)
        {
            _paymentQueue = paymentQueue;
        }

        // Add these new public methods
        public void AddToDefaultSummary(PaymentSummaryData payment)
        {
            paymentSummaryDataDefault.Add(payment);
        }

        public void AddToErrorList(PaymentSummaryData payment)
        {
            errorPaymentsList.Add(payment);
        }

        public void AddToFallbackSummary(PaymentSummaryData payment)
        {
            paymentSummaryDataFallback.Add(payment);
        }

        public PaymentsSummaryResponseDTO GetPaymentsSummary(DateTime from, DateTime to)
        {
            var defaultData = paymentSummaryDataDefault.Where(p => p.requestedAt >= from && p.requestedAt <= to);
            var fallbackData = paymentSummaryDataFallback.Where(p => p.requestedAt >= from && p.requestedAt <= to);

            var defaultSummary = new PaymentsSummaryData
            {
                totalRequests = defaultData.Count(),
                totalAmount = defaultData.Sum(p => p.amount)
            };
            var fallbackSummary = new PaymentsSummaryData
            {
                totalRequests = fallbackData.Count(),
                totalAmount = fallbackData.Sum(p => p.amount)
            };

            return new PaymentsSummaryResponseDTO
            {
                defaultData = defaultSummary,
                fallbackData = fallbackSummary
            };
        }

        public async Task ProcessPayment(Guid correlationId, float amount)
        {
            var request = new PaymentSummaryData
            {
                requestedAt = DateTime.UtcNow,
                amount = amount,
                correlationId = correlationId
            };
            paymentsList.Add(request);

            if (_paymentQueue != null)
            {
                await _paymentQueue.QueuePaymentAsync(request);
            }
        }
    }
}