namespace RenTN.Domain.Common;

public class Constants
{
    public const int PageSize = 10;
    public const string DateFormat = "yyyy-MM-dd";
}

public enum ResourceOperation
{
    Create, Update, Delete, Read
}
public static class UserRoles
{
    public const string User = "User";
    public const string Owner = "Owner";
    public const string Admin = "Admin";
}