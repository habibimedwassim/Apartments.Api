namespace RenTN.Domain.Common;

public static class EmailNormalizer
{
    public static string NormalizeEmail(string email)
    {
        if (email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase) ||
            email.EndsWith("@googlemail.com", StringComparison.OrdinalIgnoreCase))
        {
            var atIndex = email.IndexOf('@');
            var username = email.Substring(0, atIndex).Replace(".", string.Empty);
            var domain = email.Substring(atIndex);

            return username + domain;
        }

        return email;
    }
}