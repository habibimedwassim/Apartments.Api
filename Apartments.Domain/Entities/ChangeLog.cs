namespace Apartments.Domain.Entities;

public class ChangeLog
{
    public int Id { get; init; }
    public string EntityType { get; init; } = default!;
    public string PropertyName { get; init; } = default!;
    public string PropertyId { get; init; } = default!;
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public DateTime ChangedAt { get; init; } = DateTime.UtcNow;
    public string ChangedBy { get; init; } = default!;
}