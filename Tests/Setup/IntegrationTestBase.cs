using Mongo2Go;
using MongoDB.Driver;

namespace Tests.Setup;

public abstract class IntegrationTestBase : IDisposable
{
    private readonly MongoDbRunner _runner;
    protected readonly IMongoDatabase Db;
    protected readonly MongoClient Client;
    private const string DatabaseName = "IntegrationTestDb";

    protected IntegrationTestBase()
    {
        _runner = MongoDbRunner.Start();
        Client = new MongoClient(_runner.ConnectionString);
        Db = Client.GetDatabase(DatabaseName);
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}