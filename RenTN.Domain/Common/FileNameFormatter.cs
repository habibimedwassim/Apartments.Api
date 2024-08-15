namespace RenTN.Domain.Common;

public static class FileNameFormatter
{
    private static Random random = new Random();

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray()).ToLower();
    }
    public static string Format(string fileName)
    {
        var today = DateTime.Today.ToString("ddmmyyyy");
        var extension = Path.GetExtension(fileName);
        var randomString = RandomString(5);

        return $"{randomString}_{today}{extension}";
    }
}
