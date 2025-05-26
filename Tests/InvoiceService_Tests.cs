using EmailServiceProvider;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using InvoiceServiceProvider;
using InvoiceServiceProvider.Dtos;
using InvoiceServiceProvider.Factories;
using InvoiceServiceProvider.MongoDb;
using InvoiceServiceProvider.Services;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Tests;

public class InvoiceService_Tests
{
    private readonly InvoiceService _invoiceService;
    private readonly IInvoicesRepository _invoicesRepository = Substitute.For<IInvoicesRepository>();
    private readonly IPdfService _pdfService = Substitute.For<IPdfService>();
    private readonly EmailFactory _emailFactory;
    private readonly IConfiguration _configuration;
    private readonly EmailServicer.EmailServicerClient  _emailServicerClient = Substitute.For<EmailServicer.EmailServicerClient>();
    private readonly ServerCallContext _serverCallContext = Substitute.For<ServerCallContext>();
    
    public InvoiceService_Tests()
    {
        _configuration = Substitute.For<IConfiguration>();
        _configuration["Email:SenderAddress"]
            .Returns("no-reply@invoices.example.com");
        
        _emailFactory = new EmailFactory(_configuration);
        _invoiceService = new InvoiceService(_invoicesRepository, _pdfService, _emailServicerClient, _emailFactory);
    }
    
    #region Create Tests

    [Fact]
    public async Task CreateInvoice_ShouldReturnTrueAndPassAllSteps_WithValidInputData()
    {
        //Arrange
        var request = new RequestCreateInvoice
        {
            //AI generated mock data
            BookingId = "BK-20250601-0001",
            FirstName = "Alice",
            LastName = "Andersson",
            PhoneNumber = "+46 70 123 45 67",
            Email = "alice.andersson@example.com",
            Address = "Storgatan 10",
            PostalCode = "111 22",
            City = "Stockholm",
            EventName = "Midsummer Festival",
            EventDate = Timestamp.FromDateTime(
                DateTime.SpecifyKind(
                    new DateTime(2025, 6, 21, 18, 0, 0),
                    DateTimeKind.Utc)),
            TicketAmount = 2.0,
            TicketPrice = 299.00,
            BookingDate = Timestamp.FromDateTime(
                DateTime.SpecifyKind(
                    DateTime.UtcNow.AddDays(-1),
                    DateTimeKind.Utc))
        };

        var invoiceEntity = new InvoiceEntity()
        {
            Id = Guid.NewGuid().ToString(),
            //AI generated mock data
            BookingId = request.BookingId,
            FirstName = "Alice",
            LastName = "Johnson",
            PhoneNumber = "+46 70 123 45 67",
            Email = "alice.johnson@example.com",
            Address = "Storgatan 10",
            PostalCode = "111 22",
            City = "Stockholm",
            EventName = "Midsummer Festival",
            EventDate = new DateTime(2025, 7, 18, 19, 30, 0, DateTimeKind.Utc),
            TicketAmount = 2m,
            TicketPrice = 450.00m,
            TotalPrice = 900.00m,
            BookingDate = new DateTime(2025, 5, 25, 14, 0, 0, DateTimeKind.Utc),
            CreatedDate = new DateTime(2025, 5, 24, 10, 0, 0, DateTimeKind.Utc),
            DueDate = new DateTime(2025, 6, 7, 0, 0, 0, DateTimeKind.Utc),
            Paid = false,
            Deleted = false
        };

        var pdfResult = new PdfServiceResult
        {
            Succeeded = true,
            Uri = "https://www.domain.com/invoice.pdf"
        };

        var emailResult = new EmailReply { Succeeded = true };

        var response = new MongoResult
        {
            Succeeded = true,
        };
        
        //AI generated to get a mock working on the grpc call for the SendEmailAsync
        var asyncUnaryCall = new AsyncUnaryCall<EmailReply>(
            Task.FromResult(emailResult),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { }
        );
        
        _invoicesRepository.SaveInvoiceAsync(Arg.Any<InvoiceEntity>()).Returns(response);

        _pdfService.GeneratePdfAsync(Arg.Any<InvoiceEntity>()).Returns(Task.FromResult(pdfResult));

        var emailFactoryReturn = _emailFactory.CreateEmailRequest(invoiceEntity, pdfResult.Uri);
        
        _emailServicerClient
            .SendEmailAsync(
                emailFactoryReturn,  
                Arg.Any<Metadata>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>()
            )
            .ReturnsForAnyArgs(asyncUnaryCall);
        
        //Act
        var result = await _invoiceService.CreateInvoice(request, _serverCallContext);
        
        //Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    #endregion

    #region Get tests
    
    [Fact]
    public async Task GetInvoiceByInvoiceId_ShouldReturnInvoice_WithValidInvoiceId()
    {
        //Arrange
        var request = new RequestInvoiceById
        {
            Id = Guid.NewGuid().ToString(),
        };

        var bookingId = "BK-20250526-0001";

        var response = new MongoResult
        {
            Succeeded = true,
            Invoice = new InvoiceEntity
            {
                Id = request.Id,
                //AI generated mock data
                BookingId    = bookingId,
                FirstName    = "Alice",
                LastName     = "Johnson",
                PhoneNumber  = "+46 70 123 45 67",
                Email        = "alice.johnson@example.com",
                Address      = "Storgatan 10",
                PostalCode   = "111 22",
                City         = "Stockholm",
                EventName    = "Summer Jazz Concert",
                EventDate    = new DateTime(2025, 7, 18, 19, 30, 0, DateTimeKind.Utc),
                TicketAmount = 2m,
                TicketPrice  = 450.00m,
                TotalPrice   = 900.00m,
                BookingDate  = new DateTime(2025, 5, 25, 14, 0, 0, DateTimeKind.Utc),
                CreatedDate  = new DateTime(2025, 5, 24, 10, 0, 0, DateTimeKind.Utc),
                DueDate      = new DateTime(2025, 6, 7, 0, 0, 0, DateTimeKind.Utc),
                Paid         = false,
                Deleted      = false
            }
        };
        
        _invoicesRepository.GetInvoiceByInvoiceIdAsync(request.Id).Returns(response);
        
        //Act
        var result = await _invoiceService.GetInvoiceByInvoiceId(request, _serverCallContext);
        
        //Assert
        Assert.NotNull(result);
        Assert.Equal(request.Id, result.Invoice.InvoiceId);
        Assert.Equal(bookingId, response.Invoice.BookingId);
    }

    [Fact]
    public async Task GetInvoiceByInvoiceId_ShouldReturnFalse_WithInvalidInvoiceId()
    {
        //Arrange
        var request = new RequestInvoiceById
        {
            Id = Guid.NewGuid().ToString(),
        };
        
        _invoicesRepository.GetInvoiceByInvoiceIdAsync(Arg.Any<string>())
            .Returns(new MongoResult { Succeeded = false });

        //Act
        var result = await _invoiceService.GetInvoiceByInvoiceId(request, _serverCallContext);

        //Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Invoice);
    }

    [Fact]
    public async Task GetInvoiceByBookingId_ShouldReturnInvoice_WithValidInvoiceId()
    {
        //Arrange
        var bookingId = "BK-20250526-0001";

        var request = new RequestInvoiceById
        {
            Id = bookingId
        };

        var response = new MongoResult
        {
            Succeeded = true,
            Invoice = new InvoiceEntity
            {
                Id = Guid.NewGuid().ToString(),
                BookingId = bookingId,
                //AI generated mock data
                FirstName = "Alice",
                LastName = "Johnson",
                PhoneNumber = "+46 70 123 45 67",
                Email = "alice.johnson@example.com",
                Address = "Storgatan 10",
                PostalCode = "111 22",
                City = "Stockholm",
                EventName = "Summer Jazz Concert",
                EventDate = new DateTime(2025, 7, 18, 19, 30, 0, DateTimeKind.Utc),
                TicketAmount = 2m,
                TicketPrice = 450.00m,
                TotalPrice = 900.00m,
                BookingDate = new DateTime(2025, 5, 25, 14, 0, 0, DateTimeKind.Utc),
                CreatedDate = new DateTime(2025, 5, 24, 10, 0, 0, DateTimeKind.Utc),
                DueDate = new DateTime(2025, 6, 7, 0, 0, 0, DateTimeKind.Utc),
                Paid = false,
                Deleted = false
            }
        };
        
        _invoicesRepository.GetInvoiceByBookingIdAsync(bookingId).Returns(response);
        
        //Act
        var result = await _invoiceService.GetInvoiceByBookingId(request, _serverCallContext);
        
        //Assert
        Assert.NotNull(result);
        Assert.Equal(bookingId, result.Invoice.BookingId);
    }
    
    [Fact]
    public async Task GetInvoiceByBookingId_ShouldReturnFalse_WithInvalidBookingId()
    {
        // Arrange
        var request = new RequestInvoiceById
        {
            Id = Guid.NewGuid().ToString()
        };

        _invoicesRepository
            .GetInvoiceByBookingIdAsync(Arg.Any<string>())
            .Returns(Task.FromResult(new MongoResult { Succeeded = false }));

        // Act
        var result = await _invoiceService.GetInvoiceByBookingId(request, _serverCallContext);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Invoice);
    }

    [Fact]
    public async Task GetAllInvoices_ShouldReturnAllInvoices()
    {
        //Arrange
        var request = new Empty();

        var entities = new List<InvoiceEntity>
        {
            new InvoiceEntity
            {
                Id = "1234-1234-1234",
                BookingId = "BK-20250526-0001",
                //AI generated mock data
                FirstName = "Alice",
                LastName = "Johnson",
                PhoneNumber = "+46 70 123 45 67",
                Email = "alice.johnson@example.com",
                Address = "Storgatan 10",
                PostalCode = "111 22",
                City = "Stockholm",
                EventName = "Summer Jazz Concert",
                EventDate = new DateTime(2025, 7, 18, 19, 30, 0, DateTimeKind.Utc),
                TicketAmount = 2m,
                TicketPrice = 450.00m,
                TotalPrice = 900.00m,
                BookingDate = new DateTime(2025, 5, 25, 14, 0, 0, DateTimeKind.Utc),
                CreatedDate = new DateTime(2025, 5, 24, 10, 0, 0, DateTimeKind.Utc),
                DueDate = new DateTime(2025, 6, 7, 0, 0, 0, DateTimeKind.Utc),
                Paid = false,
                Deleted = false
            }
        };
        
        _invoicesRepository.GetAllAsync().Returns(new MongoResult { Succeeded = true , InvoiceList = entities });
        
        //Act
        var result = await _invoiceService.GetAllInvoices(request, _serverCallContext);
        
        //Assert
        Assert.NotNull(result);
        Assert.Single(result.AllInvoices);
    }
    #endregion
}