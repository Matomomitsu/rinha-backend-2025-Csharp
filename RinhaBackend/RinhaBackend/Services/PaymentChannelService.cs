using RinhaBackend.Models;
using System.Net;
using System.Threading.Channels;

namespace RinhaBackend.Services
{
    public class PaymentChannelService : BackgroundService, IPaymentQueue
    {
        private readonly Channel<PaymentSummaryData> _channel;
        private readonly ChannelWriter<PaymentSummaryData> _writer;
        private readonly ChannelReader<PaymentSummaryData> _reader;
        private readonly ILogger<PaymentChannelService> _logger;
        private readonly PaymentsServices _paymentsServices;

        public PaymentChannelService(ILogger<PaymentChannelService> logger, PaymentsServices paymentsServices)
        {
            _logger = logger;
            _paymentsServices = paymentsServices;
            var options = new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            };

            _channel = Channel.CreateBounded<PaymentSummaryData>(options);
            _writer = _channel.Writer;
            _reader = _channel.Reader;
        }

        public async ValueTask QueuePaymentAsync(PaymentSummaryData payment)
        {
            //_logger.LogInformation("Queueing payment {CorrelationId} for background processing", payment.correlationId);
            await _writer.WriteAsync(payment);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //_logger.LogInformation("PaymentChannelService background processor started");
            
            await foreach (var payment in _reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessPaymentAsync(payment);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing payment {CorrelationId}", payment.correlationId);
                }
            }
        }

        private async Task ProcessPaymentAsync(PaymentSummaryData payment)
        {
            //_logger.LogInformation("Processing payment {CorrelationId} for amount {Amount}", payment.correlationId, payment.amount);

            try
            {
                using (var client = new HttpClient())
                {
                    var baseUrl = Environment.GetEnvironmentVariable("PaymentProcessorDefault");
                    client.BaseAddress = new Uri(string.IsNullOrEmpty(baseUrl) ? "http://localhost:8001/payments" : baseUrl + "/payments");
                    var response = await client.PostAsJsonAsync("", new { correlationId = payment.correlationId, amount = payment.amount, requestedAt = payment.requestedAt });

                    if (response.IsSuccessStatusCode)
                    {
                        //_logger.LogInformation("Payment {CorrelationId} processed successfully", payment.correlationId);
                        _paymentsServices.AddToDefaultSummary(payment);
                    }
                    else
                    {
                        //_logger.LogWarning("Payment {CorrelationId} failed with status {StatusCode}", payment.correlationId, response.StatusCode);
                        if (response.StatusCode != (HttpStatusCode)422)
                        {
                            _paymentsServices.AddToErrorList(payment);
                        }
                        else
                        {
                            _paymentsServices.AddToFallbackSummary(payment);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment {CorrelationId}", payment.correlationId);
                _paymentsServices.AddToErrorList(payment);
                throw;
            }
        }
    }
}