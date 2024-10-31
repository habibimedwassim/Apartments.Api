namespace Apartments.Domain.Common;

public class JwtSettings
{
    public string Key { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
}

public class SmtpSettings
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class AzureBlobStorageSettings
{
    public string ConnectionString { get; set; } = default!;
    public string ContainerName { get; set; } = default!;
}
public class SendGridSettings
{
    public string ApiKey { get; set; } = default!;
    public string FromEmail { get; set; } = default!;
    public string FromName { get; set; } = default!;
}
public class FcmSettings
{
    public string ServerKey { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
}