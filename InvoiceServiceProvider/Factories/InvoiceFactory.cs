using InvoiceServiceProvider.MongoDb;

namespace InvoiceServiceProvider.Factories
{
    public class InvoiceFactory
    {
        public static InvoiceEntity? ToInvoiceEntity(RequestCreateInvoice request)
        {
            if (request is null)
                return null;

            InvoiceEntity newEntity = new()
            {
                BookingId = request.BookingId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                PostalCode = request.PostalCode,
                City = request.City,
                EventName = request.EventName,
                EventDate = TimeStampFactory.ToDateTime(request.EventDate),
                TicketAmount = request.TicketAmount,
                TicketPrice = request.TicketPrice,
                TotalPrice = request.TicketAmount * request.TicketPrice,
                BookingDate = TimeStampFactory.ToDateTime(request.BookingDate)
            };
            return newEntity;
        }

        public static Invoice? ToInvoiceGrpcModel(InvoiceEntity entity)
        {
            if (entity is null)
                return null;

            Invoice model = new()
            {
                InvoiceId = entity.Id,
                BookingId = entity.BookingId,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                PhoneNumber = entity.PhoneNumber,
                Address = entity.Address,
                PostalCode = entity.PostalCode,
                City = entity.City,
                EventName = entity.EventName,
                EventDate = TimeStampFactory.ToTimeStamp(entity.EventDate),
                TicketAmount = entity.TicketAmount,
                TicketPrice = entity.TicketPrice,
                TotalPrice = entity.TotalPrice,
                BookingDate = TimeStampFactory.ToTimeStamp(entity.BookingDate),
                CreatedDate = TimeStampFactory.ToTimeStamp(entity.CreatedDate),
                DueDate = TimeStampFactory.ToTimeStamp(entity.DueDate),
                Paid = entity.Paid
            };
            return model;
        }

        public static InvoiceEntity? ToUpdateInvoiceEntity(Invoice request)
        {
            if (request is null)
                return null;

            InvoiceEntity entity = new()
            {
                Id = request.InvoiceId,
                BookingId = request.BookingId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                PostalCode = request.PostalCode,
                City = request.City,
                EventName = request.EventName,
                EventDate = TimeStampFactory.ToDateTime(request.EventDate),
                TicketAmount = request.TicketAmount,
                TicketPrice = request.TicketPrice,
                TotalPrice = request.TotalPrice,
                BookingDate = TimeStampFactory.ToDateTime(request.BookingDate),
                CreatedDate = TimeStampFactory.ToDateTime(request.CreatedDate),
                DueDate = TimeStampFactory.ToDateTime(request.DueDate),
                Paid = request.Paid
            };
            return entity;
        }
    }
}