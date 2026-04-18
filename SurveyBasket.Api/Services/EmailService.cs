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
            Sender = new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail),
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

        try
        {
            _logger.LogInformation("Sending email to {email}", email);

            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);

            await smtp.SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {email}", email);
            throw;
        }
        finally
        {
            if (smtp.IsConnected)
                await smtp.DisconnectAsync(true);
        }
    }
}