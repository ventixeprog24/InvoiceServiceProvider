Partly AI Generated.

# Invoice Service Provider

This Microservice will generate and create invoices, save them to a NoSql-database (MongoDb) with full CRUD functionality. Upon the creation of an invoice, this service will also create a PDF invoice that is stored on Azure Blob Storage and send an email (via the email service) to the person making a booking.

This Microservice is also connected to an Azure Service Bus Queue listening for messages which enables a smoother transaction between the Booking service and the creation of invoices.

## How to use it

Make sure you have an exact copy of the proto-file in the client application. Don't forget to register the proto in the project.

### Install following packages in the client:
- Google.Protobuf
- Grpc.Net.Client
- Grpc.Net.ClientFactory
- Grpc.Tools

### Example use
```
  // 1. Create a gRPC channel & client
        var channel = new Channel("localhost:5001", ChannelCredentials.Insecure);
        var client = new InvoiceServiceContract.InvoiceServiceContractClient(channel);

        // 2. Create an invoice
        var createReply = await client.CreateInvoiceAsync(new RequestCreateInvoice
        {
            BookingId     = "BKG-12345",
            FirstName     = "Alice",
            LastName      = "Anderson",
            PhoneNumber   = "+46123456789",
            Address       = "123 Main St",
            PostalCode    = "11122",
            City          = "Stockholm",
            EventName     = "Spring Concert",
            EventDate     = Timestamp.FromDateTime(DateTime.UtcNow.AddMonths(1)),
            TicketAmount  = 2,
            TicketPrice   = 150.00,
            BookingDate   = Timestamp.FromDateTime(DateTime.UtcNow)
        });

        // 3. Retrieve it by ID
        if (createReply.Succeeded)
        {
            // Assume the repository returned/generated the invoice_id "INV-0001"
            var getReply = await client.GetInvoiceByIdAsync(new RequestInvoiceById
            {
                InvoiceId = "INV-0001"
            });

            if (getReply.Succeeded)
            {
                Console.WriteLine($"Found invoice for {getReply.Invoice.FirstName} {getReply.Invoice.LastName}, total {getReply.Invoice.TotalPrice:C}");
            }
            else
            {
                Console.WriteLine("Invoice not found.");
            }
        }

        // 4. Call the GetAllInvoices RPC
            var allReply = await client.GetAllInvoicesAsync(new Empty());

            if (!allReply.Succeeded)
            {
                Console.Error.WriteLine("Failed to retrieve invoices.");
                return;
            }

            var invoices = allReply.AllInvoices
                                     .Select(InvoiceFactory.ToInvoiceViewModel)
                                     .ToList();

        // 5. Update an invoice
        var updateInvoice = new Invoice
        {
            InvoiceId    = "INV-0001",
            BookingId    = "BKG-12345",
            FirstName    = "Alice",
            LastName     = "Anderson",
            PhoneNumber  = "+46123456789",
            Address      = "123 Main St",
            PostalCode   = "11122",
            City         = "Stockholm",
            EventName    = "Spring Concert",
            EventDate    = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(5),
            TicketAmount = 2,
            TicketPrice  = 150.00,
            TotalPrice   = 300.00,
            BookingDate  = Timestamp.FromDateTime(DateTime.UtcNow),
            CreatedDate  = Timestamp.FromDateTime(DateTime.UtcNow),
            DueDate      = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(30)),
            Paid         = true,
            Deleted      = false
        };
        var updateReply = await client.UpdateInvoiceAsync(updateInvoice);
        Console.WriteLine($"UpdateInvoice succeeded? {updateReply.Succeeded}");

        // 6. Delete an invoice
        var deleteReply = await client.DeleteInvoiceAsync(new DeleteInvoiceByIdRequest
        {
            InvoiceId = "INV-0001"
        });
        Console.WriteLine($"DeleteInvoice succeeded? {deleteReply.Succeeded}");

        await channel.ShutdownAsync();
```

### Possible Returns
| RPC Method         | Reply Message             | Field         | Possible Values             | Meaning                                                                 |
| ------------------ | ------------------------- | ------------- | --------------------------- | ----------------------------------------------------------------------- |
| **CreateInvoice**  | `CreateInvoiceReply`      | `Succeeded`   | `true`                      | Invoice was successfully created and saved.                             |
|                    |                           |               | `false`                     | Validation failed, factory returned null, or repository save failed.    |
| **GetInvoiceById** | `RequestInvoiceByIdReply` | `Succeeded`   | `true`                      | An invoice with the given ID was found and converted.                   |
|                    |                           |               | `false`                     | No invoice with that ID, conversion failed, or repository call failed.  |
|                    |                           | `Invoice`     | populated                   | The retrieved invoice (only when `Succeeded == true`).                  |
|                    |                           |               | *absent / default instance* | When `Succeeded == false`, `Invoice` will be the default/empty model.   |
| **GetAllInvoices** | `AllInvoicesReply`        | `Succeeded`   | `true`                      | All invoices were fetched successfully.                                 |
|                    |                           |               | `false`                     | Repository call failed.                                                 |
|                    |                           | `AllInvoices` | list of `Invoice` models    | Populated when `Succeeded == true` (may be empty if no invoices exist). |
| **UpdateInvoice**  | `UpdateInvoiceReply`      | `Succeeded`   | `true`                      | Invoice payload was valid and update succeeded.                         |
|                    |                           |               | `false`                     | Input was null, mapping failed, or repository update failed.            |
| **DeleteInvoice**  | `DeleteInvoiceReply`      | `Succeeded`   | `true`                      | Invoice with given ID was deleted.                                      |
|                    |                           |               | `false`                     | Input was null or repository delete failed (e.g. not found).            |
