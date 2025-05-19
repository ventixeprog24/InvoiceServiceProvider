using System;

namespace InvoiceServiceProvider.MongoDb
{
    public class InvoiceEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string BookingId { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string City { get; set; } = null!;
        public string EventName { get; set; } = null!;
        public DateTime EventDate { get; set; }
        public decimal TicketAmount { get; set; }
        public decimal TicketPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow.Date;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7).Date;
        public bool Paid { get; set; } = false;
        public bool Deleted { get; set; } = false;
    }
}