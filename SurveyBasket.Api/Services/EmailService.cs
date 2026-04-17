namespace SurveyBasket.Api.Services;

public class EmailService(IOptions<EmailSettings> mailSettings, ILogger<EmailService> logger) : IEmailSender
{
    private readonly EmailSettings _mailSettings = mailSettings.Value;
    private readonly ILogger<EmailService> _logger = logger;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // 1. Create email message (sender + subject)
        var message = new MimeMessage
        {
            Sender = MailboxAddress.Parse(_mailSettings.Mail),
            Subject = subject
        };

        // 2. Add receiver email
        message.To.Add(MailboxAddress.Parse(email));

        // 3. Build email body (HTML content)
        var builder = new BodyBuilder
        {
            HtmlBody = htmlMessage
        };

        message.Body = builder.ToMessageBody();

        // 4. Connect to SMTP server and authenticate
        using var smtp = new SmtpClient();

        _logger.LogInformation("Sending email to {email}", email);

        smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);

        // 5. Send email and disconnect
        await smtp.SendAsync(message);
        smtp.Disconnect(true);
    }
}