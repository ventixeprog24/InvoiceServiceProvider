using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using InvoiceServiceProvider.Factories;
using InvoiceServiceProvider.Helpers;
using InvoiceServiceProvider.MongoDb;
using InvoiceServiceProvider.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using QuestPDF;
using QuestPDF.Infrastructure;
using EmailServiceClient = EmailServiceProvider.EmailServicer.EmailServicerClient;

Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

//MongoDb settings
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(nameof(MongoDbSettings)));
builder.Services.AddSingleton<IMongoClient>(o =>
{
    var options = o.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(options.ConnectionString);
});
builder.Services.AddScoped(o =>
{
    var options = o.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = o.GetRequiredService<IMongoClient>();
    return client.GetDatabase(options.DatabaseName);
});

builder.Services.AddGrpcClient<EmailServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["Grpc:EmailServiceProvider"]!);
});

var blobConn  = builder.Configuration.GetConnectionString("BlobStorage");
var container = builder.Configuration["BlobStorage:ContainerName"];
builder.Services.AddSingleton(_ => new BlobContainerClient(blobConn, container));

builder.Services.AddSingleton(service => new ServiceBusClient(builder.Configuration.GetConnectionString("ServiceBus")));
builder.Services.AddHostedService<InvoiceMessageProcessor>();

builder.Services.AddScoped<EmailFactory>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<IInvoicesRepository, InvoicesRepository>();
builder.Services.AddScoped<IPdfService, PdfService>();

var app = builder.Build();

app.MapGrpcService<InvoiceService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();