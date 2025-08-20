namespace RinhaBackend.Models
{
    public class PaymentRequest
    {
        public Guid correlationId { get; set; }
        public float amount { get; set; }
    }
}
