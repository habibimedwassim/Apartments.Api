namespace RenTN.Domain.Common;

public class Constants
{
    public const int PageSize = 10;
    public const int CodeExpiration = 30;
    public const string DateFormat = "yyyy-MM-dd";
}

public static class UserRoles
{
    public const string Owner = "Owner";
    public const string Admin = "Admin";
}

public enum ResourceOperation
{
    Create, Update, Delete, Read
}
public enum VerificationCodeOperation
{
    EmailVerification,
    PasswordReset
}
public enum RentalRequestStatus
{
    Pending,
    MeetingRequested,
    MeetingScheduled,
    Approved,
    Rejected
}
public enum PaymentStatus
{
    Pending,
    Paid,
    Late
}