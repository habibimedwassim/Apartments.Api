namespace Apartments.Domain.Entities;

public class ChangeLog
{
    public int Id { get; set; }
    public string EntityType { get; set; } = default!;
    public string PropertyName { get; set; } = default!;
    public string PropertyId { get; set; } = default!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string ChangedBy { get; set; } = default!;
}