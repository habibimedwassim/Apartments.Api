namespace Apartments.Application.Dtos.AdminDtos;

public class ChangeLogDto
{
    public string EntityName { get; set; } = default!;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
}
