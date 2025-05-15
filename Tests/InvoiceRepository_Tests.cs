using InvoiceServiceProvider;
using InvoiceServiceProvider.MongoDb;
using Tests.Setup;

namespace Tests;

public class InvoiceRepository_Tests : IntegrationTestBase
{
    #region Save Tests
    [Fact]
    public async Task SaveInvoiceAsync_ShouldReturnTrue_WithValidInputEntity()
    {
        //Arrange
        var _repository = new InvoicesRepository(Db);
        InvoiceEntity invoice = new()
        {
            BookingId = "BK001",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+4685286952",
            Address = "Gatan 1",
            PostalCode = "55596",
            City = "Göteborg",
            EventName = "U2",
            EventDate = DateTime.UtcNow.AddDays(10),
            TicketAmount = 2,
            TicketPrice = 599,
            TotalPrice = 1198,
            BookingDate = DateTime.UtcNow,
        };
        
        //Act
        var result = await _repository.SaveInvoiceAsync(invoice);
        
        //Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotEmpty(result.InvoiceId!);
    }

    [Fact]
    public async Task SaveInvoiceAsync_ShouldReturnFalse_WithNullEntity()
    {
        //Arrange
        var _repository = new InvoicesRepository(Db);
        
        //Act
        var result =  await _repository.SaveInvoiceAsync(null!);
        
        //Assert
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    #endregion
    
    #region Get Tests
    
    [Fact]
    public async Task GetInvoiceByInvoiceIdAsync_ShouldReturnInvoiceEntity_WhitValidInvoiceId()
    {
        //Arrange
        var _repository = new InvoicesRepository(Db);
        InvoiceEntity invoice = new()
        {
            BookingId = "BK001",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+4685286952",
            Address = "Gatan 1",
            PostalCode = "55596",
            City = "Göteborg",
            EventName = "U2",
            EventDate = DateTime.UtcNow.AddDays(10),
            TicketAmount = 2,
            TicketPrice = 599,
            TotalPrice = 1198,
            BookingDate = DateTime.UtcNow,
        };
        var saveInvoiceResult = await _repository.SaveInvoiceAsync(invoice);
        Assert.True(saveInvoiceResult.Succeeded);
        Assert.NotEmpty(saveInvoiceResult.InvoiceId!);
        
        //Act
        var result = await _repository.GetInvoiceByInvoiceIdAsync(saveInvoiceResult.InvoiceId!);
        
        //Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Equal(saveInvoiceResult.InvoiceId, result.Invoice!.Id!);
    }

    [Theory]
    [InlineData("BK001")]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetInvoiceByInvoiceIdAsync_ShouldReturnFalse_WhitInvalidInvoiceId(string? invoiceId)
    {
        //Arrange
        var _repository = new InvoicesRepository(Db);
        
        //Act
        var result = await _repository.GetInvoiceByInvoiceIdAsync(invoiceId!);
        
        //Assert
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task GetInvoiceByBookingIdAsync_ShouldReturnInvoiceEntity_WithValidBookingId()
    {
        //Arrange
        var _repository = new InvoicesRepository(Db);
        InvoiceEntity invoice = new()
        {
            BookingId = "BK001",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+4685286952",
            Address = "Gatan 1",
            PostalCode = "55596",
            City = "Göteborg",
            EventName = "U2",
            EventDate = DateTime.UtcNow.AddDays(10),
            TicketAmount = 2,
            TicketPrice = 599,
            TotalPrice = 1198,
            BookingDate = DateTime.UtcNow,
        };
        var saveInvoiceResult = await _repository.SaveInvoiceAsync(invoice);
        Assert.True(saveInvoiceResult.Succeeded);
        Assert.NotEmpty(saveInvoiceResult.InvoiceId!);
        
        //Act
        var result = await _repository.GetInvoiceByBookingIdAsync(invoice.BookingId);
        
        //Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Equal(invoice.BookingId, result.Invoice!.BookingId);
    }

    [Theory]
    [InlineData("BK001")]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetInvoiceByBookingIdAsync_ShouldReturnFalse_WithInvalidBookingId(string? bookingId)
    {
        //Arrange
        var _repository = new InvoicesRepository(Db);
        
        //Act
        var result = await _repository.GetInvoiceByInvoiceIdAsync(bookingId!);
        
        //Assert
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllInvoices()
    {
        //Arrange
        var _repository = new InvoicesRepository(Db);
        
        //Act
        var result = await _repository.GetAllAsync();
        
        //Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    #endregion
    
    #region Update Tests
    
    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WithValidInput()
    {
        //Arrange
        var _repository = new InvoicesRepository(Db);
        InvoiceEntity invoice = new()
        {
            BookingId = "BK001",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+4685286952",
            Address = "Gatan 1",
            PostalCode = "55596",
            City = "Göteborg",
            EventName = "U2",
            EventDate = DateTime.UtcNow.AddDays(10),
            TicketAmount = 2,
            TicketPrice = 599,
            TotalPrice = 1198,
            BookingDate = DateTime.UtcNow,
        };
        var saveResult = await _repository.SaveInvoiceAsync(invoice);
        Assert.True(saveResult.Succeeded);
        Assert.NotEmpty(saveResult.InvoiceId!);
        UpdatePaymentStatusRequest updateRequest = new() { InvoiceId = saveResult.InvoiceId!, NewPaymentStatus = true };
        
        //Act
        var updateResult = await _repository.UpdateAsync(updateRequest);
        
        //Assert
        Assert.True(updateResult.Succeeded);

    }

    [Theory]
    [InlineData("INV001", true)]
    [InlineData("", true)]
    public async Task UpdateAsync_ShouldReturnFalse_WithInvalidInput(string invoiceId, bool paymentStatus)
    {
        //Arrange
        var _repository = new InvoicesRepository(Db);
        UpdatePaymentStatusRequest updateRequest = new() { InvoiceId = invoiceId, NewPaymentStatus = paymentStatus };
        
        //Act
        var updateResult = await _repository.UpdateAsync(updateRequest);
        
        //Assert
        Assert.NotNull(updateResult);
        Assert.False(updateResult.Succeeded);
    }
    #endregion
    
    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WithValidInput()
    {
        //Arrange
        var _repository = new InvoicesRepository(Db);
        InvoiceEntity invoice = new()
        {
            BookingId = "BK001",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+4685286952",
            Address = "Gatan 1",
            PostalCode = "55596",
            City = "Göteborg",
            EventName = "U2",
            EventDate = DateTime.UtcNow.AddDays(10),
            TicketAmount = 2,
            TicketPrice = 599,
            TotalPrice = 1198,
            BookingDate = DateTime.UtcNow,
        };
        var saveResult = await _repository.SaveInvoiceAsync(invoice);
        Assert.True(saveResult.Succeeded);
        Assert.NotEmpty(saveResult.InvoiceId!);
        
        //Act
        var result = await _repository.DeleteAsync(saveResult.InvoiceId!);
        
        //Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("INV001")]
    [InlineData("")]
    [InlineData(null)]
    public async Task DeleteAsync_ShouldReturnFalse_WithInvalidInput(string? invoiceId)
    {
        //Arrange
        var _repository = new InvoicesRepository(Db);
        
        //Act
        var result = await _repository.DeleteAsync(invoiceId!);
        
        //Assert
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    #endregion
}