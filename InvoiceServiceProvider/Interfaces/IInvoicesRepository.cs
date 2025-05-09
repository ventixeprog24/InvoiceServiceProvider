using InvoiceServiceProvider.Dtos;

namespace InvoiceServiceProvider.MongoDb;

public interface IInvoicesRepository
{
    Task<MongoResult> SaveInvoiceAsync(InvoiceEntity invoice);
    Task<MongoResult> GetInvoiceByIdAsync(string id);
    Task<MongoResult> GetAllAsync();
    Task<MongoResult> UpdateAsync(InvoiceEntity invoice);
    Task<MongoResult> DeleteAsync(string id);
}