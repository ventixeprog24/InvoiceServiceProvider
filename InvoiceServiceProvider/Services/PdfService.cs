using Azure.Storage.Blobs;
using InvoiceServiceProvider.Dtos;
using InvoiceServiceProvider.Helpers;
using InvoiceServiceProvider.MongoDb;
using QuestPDF.Fluent;

namespace InvoiceServiceProvider.Services;

public class PdfService(BlobContainerClient blobContainerClient) : IPdfService
{
    private readonly BlobContainerClient _blobContainerClient = blobContainerClient;
    
    public async Task<PdfServiceResult> GeneratePdfAsync(InvoiceEntity invoice, CancellationToken cancellationToken = default)
    {   
        var document = Document.Create(container =>
        {
            new InvoiceDocumentHelper(invoice).Compose(container);
        });

        if (document is null)
            return new PdfServiceResult { Succeeded = false, Message = "Failed to generate PDF" };

        try
        {
            await using var stream = new MemoryStream();
            await Task.Run(() => document.GeneratePdf(stream), cancellationToken);
            stream.Position = 0;

            var blobPath = $"{invoice.Id}.pdf";
            var blobClient = _blobContainerClient.GetBlobClient(blobPath);
            await blobClient.UploadAsync(stream, overwrite: true, cancellationToken);
            var uri = blobClient.Uri.ToString();
            return new PdfServiceResult { Succeeded = true,  Uri = uri };
        }
        catch (Exception ex)
        {
            return new PdfServiceResult { Succeeded = false, Message = ex.Message };
        }
    }
}