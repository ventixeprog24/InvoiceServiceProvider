using System.Diagnostics;
using InvoiceServiceProvider.Dtos;
using MongoDB.Driver;

namespace InvoiceServiceProvider.MongoDb
{
    public class InvoicesRepository(IMongoDatabase db) : IInvoicesRepository
    {
        private readonly IMongoCollection<InvoiceEntity> _invoices = db.GetCollection<InvoiceEntity>("Invoices");

        public async Task<MongoResult> SaveInvoiceAsync(InvoiceEntity invoice)
        {
            try
            {
                await _invoices.InsertOneAsync(invoice);
                return new MongoResult { Succeeded = true, InvoiceId = invoice.Id};
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new MongoResult { Succeeded = false };
            }
        }

        public async Task<MongoResult> GetInvoiceByIdAsync(string id)
        {
            try
            {
                var invoice = await _invoices.Find(i => i.Id == id).FirstOrDefaultAsync();
                return invoice is null 
                    ? new MongoResult { Succeeded = false }
                    : new MongoResult { Succeeded = true, Invoice = invoice };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new MongoResult { Succeeded = false };
            }
        }

        public async Task<MongoResult> GetAllAsync()
        {
            try
            {
                var result = await _invoices.Find(_ => true).ToListAsync();
                if (result is not null)
                    return new MongoResult { Succeeded = true, InvoiceList = result };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return new MongoResult { Succeeded = false };
        }

        public async Task<MongoResult> UpdateAsync(InvoiceEntity invoice)
        {
            try
            {
                var result = await _invoices.ReplaceOneAsync(i => i.Id == invoice.Id, invoice);
                if (result.IsAcknowledged && result.ModifiedCount > 0)
                    return new MongoResult { Succeeded = true };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return new MongoResult { Succeeded = false };
        }

        public async Task<MongoResult> DeleteAsync(string id)
        {
            try
            {
                var result = await _invoices.DeleteOneAsync(i => i.Id == id);
                if (result.IsAcknowledged && result.DeletedCount > 0)
                    return new MongoResult { Succeeded = true };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return new MongoResult { Succeeded = false };
        }
    }
}
