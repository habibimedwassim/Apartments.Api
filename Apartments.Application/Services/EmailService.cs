using Apartments.Application.IServices;
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using SendGrid;
using Apartments.Domain.Common;
using System.Net.Mail;
using System.Net;

namespace Apartments.Application.Services;
public class EmailService(IOptions<SendGridSettings> sendGridSettings) : IEmailService
{
    private readonly SendGridSettings _sendGridSettings = sendGridSettings.Value;

    //public async Task SendEmailAsync(string to, string subject, string body)
    //{
    //    var client = new SendGridClient(_sendGridSettings.ApiKey);
    //    var from = new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.FromName);
    //    var toEmail = new EmailAddress(to);
    //    var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, body, body);

    //    var response = await client.SendEmailAsync(msg);

    //    if (!response.IsSuccessStatusCode)
    //    {
    //        throw new Exception($"Failed to send email. Status Code: {response.StatusCode}");
    //    }
    //}
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var client = new SendGridClient(_sendGridSettings.ApiKey);
        var from = new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.FromName);
        var toEmail = new EmailAddress(to);
        var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, plainTextContent: body, htmlContent: body);

        var response = await client.SendEmailAsync(msg);

        if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
        {
            throw new Exception($"Failed to send email. Status code: {response.StatusCode}");
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