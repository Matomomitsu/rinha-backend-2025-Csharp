namespace RinhaBackend.Models
{
    public class PaymentSummaryData
    {
        public DateTime requestedAt { get; set; }
        public float amount { get; set; }
        public Guid correlationId { get; set; }
    }
}
