using InvoiceServiceProvider.Dtos;

namespace InvoiceServiceProvider.MongoDb;

public interface IPdfService
{
    Task<PdfServiceResult> GeneratePdfAsync(InvoiceEntity invoice, CancellationToken cancellationToken = default);
}