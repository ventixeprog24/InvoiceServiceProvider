using InvoiceServiceProvider.MongoDb;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InvoiceServiceProvider.Helpers;

public class InvoiceDocumentHelper(InvoiceEntity invoice) : IDocument
{
    private readonly InvoiceEntity _invoice =  invoice;
    
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(12));

            page.Header().Row(row =>
            {
                row.ConstantColumn(250)
                    .Column(column =>
                    {
                        column.Item().Text("Ventixe").SemiBold().FontSize(18);
                        column.Item().Text("Valhallavägen 17");
                        column.Item().Text("Stockholm");
                    });

                row.RelativeColumn().AlignRight().Column(column =>
                {
                    column.Item().Text($"Invoice #: {_invoice.Id}").SemiBold();
                    column.Item().Text($"Date: {_invoice.CreatedDate:yyyy-MM-dd}");
                    column.Item().Text($"Due Date: {_invoice.DueDate:yyyy-MM-dd}");
                    column.Item().Text($"Booking Id: {_invoice.BookingId}");
                    column.Item().Text($"Name: {_invoice.FirstName} {_invoice.LastName}").SemiBold();
                    column.Item().Text($"Address: {_invoice.Address}");
                    column.Item().Text($"Postal Code: {_invoice.PostalCode}");
                    column.Item().Text($"City: {_invoice.City}");
                });
            });

            page.Content().Column(content =>
            {
                content.Spacing(5);

                content.Item().Text("Details:").Bold();
                content.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.ConstantColumn(50);
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(80);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Event").SemiBold();
                        header.Cell().Text("Event Date").SemiBold();
                        header.Cell().Text("Qty").SemiBold();
                        header.Cell().Text("Price").SemiBold();
                        header.Cell().Text("Total").SemiBold();
                    });
                    
                    table.Cell().Text(_invoice.EventName);
                    table.Cell().Text($"{_invoice.EventDate:yyyy-MM-dd}");
                    table.Cell().AlignRight().Text(_invoice.TicketAmount.ToString());
                    table.Cell().AlignRight().Text(_invoice.TicketPrice.ToString());;
                    table.Cell().AlignRight().Text(_invoice.TotalPrice.ToString());
                    
                    table.Footer(footer =>
                    {
                        footer.Cell().ColumnSpan(3).AlignRight().Text("Total: ").Bold();
                        footer.Cell().AlignRight().Text(_invoice.TotalPrice.ToString()).SemiBold();
                    });
                });
            });
        });
    }
}