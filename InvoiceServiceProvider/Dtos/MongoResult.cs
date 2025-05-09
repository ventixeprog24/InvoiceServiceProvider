using InvoiceServiceProvider.MongoDb;

namespace InvoiceServiceProvider.Dtos
{
    public class MongoResult
    {
        public bool Succeeded { get; set; }
        public string? InvoiceId { get; set; }
        public InvoiceEntity? Invoice { get; set; }
        public IEnumerable<InvoiceEntity>? InvoiceList { get; set; }
    }
}
