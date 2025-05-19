namespace InvoiceServiceProvider.Dtos;

public class PdfServiceResult
{
    public bool Succeeded { get; set; }
    public string? Uri { get; set; }
    public string? Message { get; set; }
}