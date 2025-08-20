using Microsoft.AspNetCore.Builder;
using RinhaBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Register services in correct order to avoid circular dependencies
builder.Services.AddSingleton<PaymentsServices>();
builder.Services.AddSingleton<PaymentChannelService>();
builder.Services.AddSingleton<IPaymentQueue>(provider => provider.GetService<PaymentChannelService>()!);
builder.Services.AddHostedService(provider => 
{
    var paymentChannelService = provider.GetService<PaymentChannelService>()!;
    var paymentsServices = provider.GetService<PaymentsServices>()!;
    
    // Wire up the dependency after both services are created
    paymentsServices.SetPaymentQueue(paymentChannelService);
    
    return paymentChannelService;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI();
    app.UseSwagger();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
