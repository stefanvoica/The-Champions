using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SendWelcomeEmail(string toEmail)
    {
        var settings = _configuration.GetSection("EmailSettings");
        var from = settings["From"];
        var smtp = settings["SmtpServer"];
        var port = int.Parse(settings["Port"]);
        var user = settings["Username"];
        var pass = settings["Password"];

        var message = new MailMessage(from, toEmail)
        {
            Subject = "🎉 Bun venit în comunitatea Pro Cosmetic!",
            Body = @"Salut!

Îți mulțumim că te-ai abonat la newsletterul Pro Cosmetic.

De acum înainte vei fi printre primii care află despre:
- 🎁 reduceri exclusive
- ✨ produse noi
- 💡 sfaturi și trucuri de îngrijire

Ne bucurăm să te avem alături!

Cu drag,  
Echipa Pro Cosmetic",
            IsBodyHtml = false
        };



        using var client = new SmtpClient(smtp, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true
        };

        client.Send(message);
    }
}
