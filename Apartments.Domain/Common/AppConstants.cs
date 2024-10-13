namespace Apartments.Domain.Common;

public static class AppConstants
{
    public const int PageSize = 20;
    public const int CodeExpiration = 10;
    public const int TokenExpiration = 30;
    public const int PhotosLimit = 4;
    public const string DateFormat = "yyyy-MM-dd";
    public const string TempAdmin = "admin@temp.com";
}

public static class UserRoles
{
    public const string Owner = "Owner";
    public const string Admin = "Admin";
    public const string User = "User";
}

public static class RequestStatus
{
    public const string Pending = nameof(Pending);
    public const string Approved = nameof(Approved);
    public const string Rejected = nameof(Rejected);
    public const string Cancelled = nameof(Cancelled);
    public const string MeetingScheduled = nameof(MeetingScheduled);
    public const string Terminated = nameof(Terminated);
    public const string Departed = nameof(Departed);
    public const string Paid = nameof(Paid);
    public const string Late = nameof(Late);
}

public enum SortDirection
{
    Ascending,
    Descending
}

public enum ResourceOperation
{
    Create,
    Update,
    Delete,
    Restore,
    ApproveReject,
    Cancel
}

public enum VerificationCodeType
{
    Email,
    NewEmail,
    Password
}
public enum VerificationCodeOperation
{
    EmailVerification,
    VerificationCode,
    PasswordReset
}

public enum RequestType
{
    Sent,
    Received,
    All
}

public enum RequestAction
{
    Approve,
    Reject
}

public enum ApartmentRequestType
{
    Rent,
    Leave,
    Dismiss
}

public enum PaymentRequestAction
{
    Accept,
    Cancel,
    Late,
    Reset
}

public enum StatisticsType
{
    Users,
    Apartments,
    Photos,
    RentRequests,
    DismissRequests,
    LeaveRequests
}