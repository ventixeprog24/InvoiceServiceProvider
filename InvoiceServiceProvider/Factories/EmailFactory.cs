using EmailServiceProvider;
using InvoiceServiceProvider.MongoDb;

namespace InvoiceServiceProvider.Factories;

public class EmailFactory(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;
    
    public EmailRequest CreateEmailRequest(InvoiceEntity invoice, string invoiceUri)
    {
        var senderAddress = _configuration["Email:SenderAddress"];
        var subject = "Ventixe Event Invoice";
        var plainText = $@"
                            Congratulations on your purchase!
                            
                            We hope that you are excited about the upcoming event.
                            Here is the payment details.

                            Total amount: €{invoice.TotalPrice.ToString()}
                            To account number: 555.5555.555
                            Bank: Österreichische Alpenland Bank AG
                            Account number: AT65 12345 98765432101
                            Account owner: Ventixe Österreichische Stiftung für diskrete Kapitalverflechtung (ÖSDK)

                            Do you want your invoice as PDF? Use this address:
                            {invoiceUri}
                        ";
        var html = $@"
                    <!DOCTYPE html>
                    <html lang=""en"">
                    <head>
                      <meta charset=""UTF-8"">
                      <title>Thank You for Your Purchase</title>
                    </head>
                    <body style='margin:0; padding:32px; font-family: sans-serif; background-color: #F7F7F7; color:#1E1E20;'>
                        <div style='max-width: 660px; margin: 32px auto; background: #FFFFFF; border-radius: 16px; padding: 32px;'>
                          <h1>Congratulations on your purchase!</h1>

                          <p>We hope that you are excited about the upcoming event. Here are the payment details:</p>

                          <p>Total amount: {invoice.TotalPrice.ToString()}</p>
                          <p>Payment reference: {invoice.Id}</p>
                          <p>To account number: 555.5555.555</p>
                          <p>Bank: Österreichische Alpenland Bank AG</p>
                          <p>Account number: AT65 12345 98765432101</p>
                          <p>Account owner: Ventixe Österreichische Stiftung für diskrete Kapitalverflechtung (ÖSDK)</p>

                          <p>Do you want your invoice as a PDF?</p>
                          <a href=""{invoiceUri}"" style='display: inline-block; color: #39393D; border: 1px solid transparent; background-color: #F26CF9;
                            border-radius: 25px; padding: 0.9rem 1.5rem; font-size: 14px; font-weight: 500; cursor: pointer; 
                            text-decoration: none;'>Download Invoice</a>
                        </div>
                    </body>
                    </html>
                    ";

        EmailRequest emailRequest = new()
        {
            Recipients = { invoice.Email },
            SenderAddress = senderAddress,
            Subject = subject,
            PlainText = plainText,
            Html = html
        };
        return emailRequest;
    }
}