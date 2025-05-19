using Google.Protobuf.WellKnownTypes;
using InvoiceServiceProvider;
using InvoiceServiceProvider.Factories;
using InvoiceServiceProvider.MongoDb;

namespace Tests;

public class InvoiceFactory_Tests
{
    [Fact]
    //This is 90% GPT generated because i wanted to save time from writing all the asserts.
    public void ToInvoiceEntity_ShouldReturnInvoiceEntityAndMapEverythingProperly_WithValidInput()
    {
        var now = DateTime.UtcNow;
        var eventTs   = Timestamp.FromDateTime(now.AddDays(5));
        var bookingTs = Timestamp.FromDateTime(now.AddDays(-1));
        var request = new RequestCreateInvoice
        {
            BookingId    = "B123",
            FirstName    = "Jane",
            LastName     = "Roe",
            PhoneNumber  = "555-0100",
            Address      = "1 Test St",
            PostalCode   = "12345",
            City         = "Testville",
            EventName    = "Demo",
            EventDate    = eventTs,
            BookingDate  = bookingTs,
            TicketAmount = 4,
            TicketPrice  = 120.5
        };
        var expectedEventDate   = TimeStampFactory.ToDateTime(eventTs);
        var expectedBookingDate = TimeStampFactory.ToDateTime(bookingTs); 
        var ticketAmountDecimal = Convert.ToDecimal(request.TicketAmount);
        var ticketAmountNoDecimals = Math.Round(ticketAmountDecimal, 0);
        var ticketPriceDecimal = Convert.ToDecimal(request.TicketPrice);
        var ticketPriceTwoDecimals = Math.Round(ticketPriceDecimal, 2);
        var expectedTotalPrice  = ticketAmountNoDecimals * ticketPriceTwoDecimals;
        var expectedPriceTwoDecimals = Math.Round(expectedTotalPrice, 2);
        

        // Act
        var entity = InvoiceFactory.ToInvoiceEntity(request);

        // Assert
        Assert.NotNull(entity);
        Assert.IsType<InvoiceEntity>(entity);
        Assert.Equal(request.BookingId,    entity.BookingId);
        Assert.Equal(request.FirstName,    entity.FirstName);
        Assert.Equal(request.LastName,     entity.LastName);
        Assert.Equal(request.PhoneNumber,  entity.PhoneNumber);
        Assert.Equal(request.Address,      entity.Address);
        Assert.Equal(request.PostalCode,   entity.PostalCode);
        Assert.Equal(request.City,         entity.City);
        Assert.Equal(request.EventName,    entity.EventName);
        Assert.Equal(expectedEventDate,   entity.EventDate);
        Assert.Equal(expectedBookingDate, entity.BookingDate);
        Assert.Equal(ticketAmountNoDecimals,    entity.TicketAmount);
        Assert.Equal(ticketPriceTwoDecimals,     entity.TicketPrice);
        Assert.Equal(expectedPriceTwoDecimals,  entity.TotalPrice);
    }

    [Fact]
    public void ToInvoiceEntity_ShouldReturnNull_WithInvalidInput()
    {
        //Act
        var result = InvoiceFactory.ToInvoiceEntity(null!);
        
        //Assert
        Assert.Null(result);
    }

    [Fact]
    //This is 90% GPT generated because i wanted to save time from writing all the asserts.
    public void ToInvoiceGrpcModel_ShouldReturnModelWithCorrectMapping_WithValidInput()
    {
        // Arrange
            var now = DateTime.UtcNow;
            var entity = new InvoiceEntity
            {
                Id           = "E789",
                BookingId    = "B456",
                FirstName    = "Alice",
                LastName     = "Smith",
                PhoneNumber  = "555-0200",
                Address      = "2 Sample Ave",
                PostalCode   = "67890",
                City         = "Exampletown",
                EventName    = "SampleEvent",
                EventDate    = now.AddDays(10),
                BookingDate  = now.AddDays(-2),
                CreatedDate  = now.AddDays(-3),
                DueDate      = now.AddDays(30),
                TicketAmount = 3,
                TicketPrice  = 200,
                TotalPrice   = 600,
                Paid         = true,
                Deleted      = false
            };

            var expEventTs   = TimeStampFactory.ToTimeStamp(entity.EventDate);
            var expBookingTs = TimeStampFactory.ToTimeStamp(entity.BookingDate);
            var expCreatedTs = TimeStampFactory.ToTimeStamp(entity.CreatedDate);
            var expDueTs     = TimeStampFactory.ToTimeStamp(entity.DueDate);
            
            var ticketAmountDouble = Convert.ToDouble(entity.TicketAmount);
            var ticketAmountNoDecimals = Math.Round(ticketAmountDouble, 0);
            var ticketPriceDouble = Convert.ToDouble(entity.TicketPrice);
            var ticketPriceTwoDoubles = Math.Round(ticketPriceDouble, 2);
            var expectedTotalPrice  = ticketAmountNoDecimals * ticketPriceTwoDoubles;
            var expectedPriceTwoDecimals = Math.Round(expectedTotalPrice, 2);

            // Act
            var model = InvoiceFactory.ToInvoiceGrpcModel(entity);

            // Assert
            Assert.NotNull(model);
            Assert.IsType<Invoice>(model);
            Assert.Equal(entity.Id,           model.InvoiceId);
            Assert.Equal(entity.BookingId,    model.BookingId);
            Assert.Equal(entity.FirstName,    model.FirstName);
            Assert.Equal(entity.LastName,     model.LastName);
            Assert.Equal(entity.PhoneNumber,  model.PhoneNumber);
            Assert.Equal(entity.Address,      model.Address);
            Assert.Equal(entity.PostalCode,   model.PostalCode);
            Assert.Equal(entity.City,         model.City);
            Assert.Equal(entity.EventName,    model.EventName);
            Assert.Equal(expEventTs,          model.EventDate);
            Assert.Equal(ticketAmountNoDecimals, model.TicketAmount);
            Assert.Equal(ticketPriceTwoDoubles,  model.TicketPrice);
            Assert.Equal(expectedPriceTwoDecimals,   model.TotalPrice);
            Assert.Equal(expBookingTs,        model.BookingDate);
            Assert.Equal(expCreatedTs,        model.CreatedDate);
            Assert.Equal(expDueTs,            model.DueDate);
            Assert.Equal(entity.Paid,         model.Paid);
            Assert.Equal(entity.Deleted,      model.Deleted);
    }

    [Fact]
    public void ToInvoiceGrpcModel_ShouldReturnNull_WithInalidInput()
    {
        //Act
        var result =  InvoiceFactory.ToInvoiceGrpcModel(null!);
        
        //Assert
        Assert.Null(result);
    }
    
    
    
}