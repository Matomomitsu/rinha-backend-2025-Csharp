namespace RinhaBackend.Models
{

    public class PaymentsSummaryData 
    {
        public int totalRequests { get; set; }
        public float totalAmount { get; set; }
    }

    public class PaymentsSummaryResponseDTO
    {
        public PaymentsSummaryData defaultData { get; set; }
        public PaymentsSummaryData fallbackData { get; set; }
    }
}
