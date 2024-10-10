namespace Apartments.Application.IServices;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task<string> GetEmailTemplateAsync(string templateName, Dictionary<string, string> placeholders);
}