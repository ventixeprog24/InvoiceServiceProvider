using InvoiceServiceProvider.Dtos;

namespace InvoiceServiceProvider.MongoDb;

public interface IInvoicesRepository
{
    Task<MongoResult> SaveInvoiceAsync(InvoiceEntity invoice);
    Task<MongoResult> GetInvoiceByInvoiceIdAsync(string id);
    Task<MongoResult> GetInvoiceByBookingIdAsync(string id);
    Task<MongoResult> GetAllAsync();
    Task<MongoResult> UpdateAsync(UpdatePaymentStatusRequest request);
    Task<MongoResult> DeleteAsync(string id);
    Task<MongoResult> HardDeleteFromDbAsync(string id);
}