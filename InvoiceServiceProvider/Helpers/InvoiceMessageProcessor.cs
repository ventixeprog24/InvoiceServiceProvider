using Azure.Messaging.ServiceBus;
using InvoiceServiceProvider.Services;

namespace InvoiceServiceProvider.Helpers;

public class InvoiceMessageProcessor : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ServiceBusClient _serviceBusClient;
    private ServiceBusProcessor? _serviceBusProcessor;
    private readonly IServiceScopeFactory _scopeFactory;
    private string _queueName;

    public InvoiceMessageProcessor(IConfiguration configuration, ServiceBusClient serviceBusClient,
        IServiceScopeFactory scopeFactory)
    {
        _configuration = configuration;
        _serviceBusClient = serviceBusClient;
        _scopeFactory = scopeFactory;
        _queueName = _configuration["ServiceBus:QueueName"]!;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _serviceBusProcessor = _serviceBusClient.CreateProcessor(queueName: _queueName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 2,
            AutoCompleteMessages = false
        });

        _serviceBusProcessor.ProcessMessageAsync += ProcessMessageHandler;
        _serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;
        
        return _serviceBusProcessor.StartProcessingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_serviceBusProcessor is not null)
        {
            await _serviceBusProcessor.StopProcessingAsync(cancellationToken);
            await _serviceBusProcessor.DisposeAsync();
        }
    }

    private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
    {
        RequestCreateInvoice? invoice = new();
        
        try
        {
            invoice = args.Message.Body.ToObjectFromJson<RequestCreateInvoice>();
            
            if (invoice is null)
                throw new Exception("Invoice message was serialized to null");
        }
        catch (Exception ex)
        {
            await args.DeadLetterMessageAsync(args.Message, "JsonDeserializeFailure", ex.Message);
            return;
        }
        
        using var scope = _scopeFactory.CreateScope();
        var invoiceService = scope.ServiceProvider.GetRequiredService<InvoiceService>();
        
        var result = await invoiceService.CreateInvoiceFromServiceBus(invoice);
        if (!result.Succeeded)
            throw new InvalidOperationException("Invoice creation failed");
        
        await args.CompleteMessageAsync(args.Message);
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        return Task.CompletedTask;
    }
}