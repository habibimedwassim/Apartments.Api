namespace RenTN.Application.DTOs.ChangeLogDTOs;

public class DateRangeDTO
{
    public string EntityName { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; } = DateTime.UtcNow;
}
