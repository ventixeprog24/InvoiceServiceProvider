using InvoiceServiceProvider.MongoDb;
using InvoiceServiceProvider.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

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

builder.Services.AddScoped<IInvoicesRepository, InvoicesRepository>();

var app = builder.Build();

app.MapGrpcService<InvoiceService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
