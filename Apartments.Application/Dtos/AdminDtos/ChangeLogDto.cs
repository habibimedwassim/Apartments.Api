namespace Apartments.Application.Dtos.AdminDtos;

public class ChangeLogDto
{
    public string EntityName { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; } = DateTime.UtcNow;
}