using Apartments.Application.IServices;
using Microsoft.Extensions.Options;
using Apartments.Domain.Common;
using System.Net.Mail;
using System.Net;


namespace Apartments.Application.Services;
public class EmailService(
    IOptions<SmtpSettings> options) : IEmailService
{
    private readonly SmtpSettings _smtpSettings = options.Value;
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var fromAddress = new MailAddress(_smtpSettings.UserName, "RenTN");
            var toAddress = new MailAddress(to);

            using (var smtp = new SmtpClient
            {
                Host = _smtpSettings.Host,
                Port = _smtpSettings.Port,
                EnableSsl = _smtpSettings.EnableSsl,
                Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password)
            })
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                await smtp.SendMailAsync(message);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to send email: {ex.Message}", ex);
        }
    }

    public async Task<string> GetEmailTemplateAsync(string templateName, Dictionary<string, string> placeholders)
    {
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Emails", $"{templateName}.html");

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template '{templateName}' not found.");

        var templateContent = await File.ReadAllTextAsync(templatePath);

        foreach (var placeholder in placeholders)
        {
            templateContent = templateContent.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
        }

        return templateContent;
    }
}