using Apartments.Application.IServices;
using Apartments.Domain.Common;
using Microsoft.Extensions.Options;

namespace Apartments.Application.Services;

public class EmailService(IOptions<SmtpSettings> smtpSettings) : IEmailService
{
    private readonly SmtpSettings _smtpSettings = smtpSettings.Value;

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        //using var smtpClient = new SmtpClient(_smtpSettings.Host)
        //{
        //    Port = _smtpSettings.Port,
        //    Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
        //    EnableSsl = true,
        //    UseDefaultCredentials = false
        //};

        //var mailMessage = new MailMessage
        //{
        //    From = new MailAddress(_smtpSettings.From),
        //    Subject = subject,
        //    Body = body,
        //    IsBodyHtml = true,
        //};
        //mailMessage.To.Add(to);

        //try
        //{
        //    await smtpClient.SendMailAsync(mailMessage);
        //}
        //catch (SmtpException ex)
        //{
        //    throw new Exception($"Failed to send email: {ex.Message}", ex);
        //}
    }
}
