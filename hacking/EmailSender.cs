using System.Net;
using System.Net.Mail;

namespace sutnance.hacking;

public interface IEmailSender
{
    void SendAsync(string receiver,string subject, string body, CancellationToken ctx = default);
}
public class EmailSender : IEmailSender
{
    private string SenderEmail = "sasicraft55@gmail.com";
    private string Password = "unqs czxq zqst yokl";

    public void SendAsync(string receiver,string subject, string body, CancellationToken ctx = default)
    {
        var smtpclient = new SmtpClient("smtp.gmail.com", 587);
        smtpclient.Credentials = new NetworkCredential(SenderEmail, Password);
        smtpclient.EnableSsl = true;
        MailMessage mail = new MailMessage(
            new MailAddress(SenderEmail),
            new MailAddress(receiver)
            )
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        smtpclient.Send(mail);
    }
}

public class FakeEmailSender : IEmailSender
{
   public void SendAsync(string receiver, string subject, string body, CancellationToken ctx = default)
   {
       // Console.WriteLine($"From: {} ");
       Console.WriteLine($"To: {receiver} ");
       Console.WriteLine($"Suject: {subject} ");
       Console.WriteLine($"Body: {body} ");
   }
}
