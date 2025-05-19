using InvoiceServiceProvider.MongoDb;

namespace InvoiceServiceProvider.Factories
{
    public class InvoiceFactory
    {
        public static InvoiceEntity? ToInvoiceEntity(RequestCreateInvoice request)
        {
            if (request is null)
                return null;

            var ticketAmountDecimal = Convert.ToDecimal(request.TicketAmount);
            var ticketAmountNoDecimals = Math.Round(ticketAmountDecimal, 0);
            
            var ticketPriceDecimal = Convert.ToDecimal(request.TicketPrice);
            var ticketPriceTwoDecimals = Math.Round(ticketPriceDecimal, 2);
            
            var totalPriceDecimal = ticketAmountNoDecimals * ticketPriceTwoDecimals;
            var totalPriceTwoDecimals = Math.Round(totalPriceDecimal, 2);

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
                TicketAmount = ticketAmountNoDecimals,
                TicketPrice = ticketPriceTwoDecimals,
                TotalPrice = totalPriceTwoDecimals,
                BookingDate = TimeStampFactory.ToDateTime(request.BookingDate)
            };
            return newEntity;
        }

        public static Invoice? ToInvoiceGrpcModel(InvoiceEntity entity)
        {
            if (entity is null)
                return null;
            
            var ticketAmountDouble = Convert.ToDouble(entity.TicketAmount);
            var ticketAmountNoDecimals = Math.Round(ticketAmountDouble, 0);
            
            var ticketPriceDouble = Convert.ToDouble(entity.TicketPrice);
            var ticketPriceTwoDecimals = Math.Round(ticketPriceDouble, 2);
            
            var totalPriceDouble = ticketAmountNoDecimals * ticketPriceTwoDecimals;
            var totalPriceTwoDecimals = Math.Round(totalPriceDouble, 2);
            

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
                TicketAmount = ticketAmountNoDecimals,
                TicketPrice = ticketPriceTwoDecimals,
                TotalPrice = totalPriceTwoDecimals,
                BookingDate = TimeStampFactory.ToTimeStamp(entity.BookingDate),
                CreatedDate = TimeStampFactory.ToTimeStamp(entity.CreatedDate),
                DueDate = TimeStampFactory.ToTimeStamp(entity.DueDate),
                Paid = entity.Paid,
                Deleted = entity.Deleted
            };
            return model;
        }
    }
}