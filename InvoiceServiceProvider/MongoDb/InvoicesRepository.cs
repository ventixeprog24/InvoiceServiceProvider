using System.Diagnostics;
using InvoiceServiceProvider.Dtos;
using InvoiceServiceProvider.Factories;
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

        public async Task<MongoResult> GetInvoiceByInvoiceIdAsync(string id)
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

        public async Task<MongoResult> GetInvoiceByBookingIdAsync(string id)
        {
            try
            {
                var invoice = await _invoices.Find(i => i.BookingId == id).FirstOrDefaultAsync();
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

        public async Task<MongoResult> UpdateAsync(UpdatePaymentStatusRequest request)
        {
            try
            {
                var invoiceToUpdate = await _invoices.Find(i => i.Id == request.InvoiceId).FirstOrDefaultAsync();
                if (invoiceToUpdate is null)
                    return new MongoResult { Succeeded = false };

                invoiceToUpdate.Paid = request.NewPaymentStatus;

                var result = await _invoices.ReplaceOneAsync(i => i.Id == request.InvoiceId, invoiceToUpdate);
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
                var invoiceToUpdate = await _invoices.Find(i => i.Id == id).FirstOrDefaultAsync();
                if (invoiceToUpdate is null)
                    return new MongoResult { Succeeded = false };

                invoiceToUpdate.Deleted = true;
                invoiceToUpdate.BookingId = $"{invoiceToUpdate.BookingId}-{Guid.NewGuid()}";

                var result = await _invoices.ReplaceOneAsync(i => i.Id == id, invoiceToUpdate);
                if (result.IsAcknowledged && result.ModifiedCount > 0)
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
