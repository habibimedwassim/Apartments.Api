using RenTN.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace RenTN.Application.Utilities;

public static class CoreUtilities
{
    public static List<ChangeLog> GenerateChangeLogs<T>(List<T> originalList, List<T> updatedList, string changedBy, List<string>? additionalPropertiesToExclude = null)
    {
        var changeLogs = new List<ChangeLog>();

        for (int i = 0; i < originalList.Count; i++)
        {
            var original = originalList[i];
            var updated = updatedList[i];

            changeLogs.AddRange(GenerateChangeLogs(original, updated, changedBy, additionalPropertiesToExclude));
        }

        return changeLogs;
    }
    public static List<ChangeLog> GenerateChangeLogs<T>(T original, T updated, string changedBy, List<string>? additionalPropertiesToExclude = null)
    {
        var propertiesToExclude = new List<string>() { "ID", "Id" };

        if (additionalPropertiesToExclude != null)
        {
            propertiesToExclude.AddRange(additionalPropertiesToExclude);
        }

        var changeLogs = new List<ChangeLog>();

        var properties = typeof(T).GetProperties();

        // Determine the primary key property
        var keyProperty = properties.FirstOrDefault(p =>
            p.GetCustomAttribute<KeyAttribute>() != null ||
            string.Equals(p.Name, "ID", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase)
        );

        if (keyProperty == null)
        {
            throw new InvalidOperationException($"No primary key defined for entity type {typeof(T).Name}");
        }

        var primaryKeyValue = keyProperty.GetValue(original)?.ToString();

        foreach (var property in properties)
        {
            if (propertiesToExclude.Contains(property.Name)) continue;

            var originalValue = property.GetValue(original)?.ToString();
            var updatedValue = property.GetValue(updated)?.ToString();

            if (originalValue != updatedValue)
            {
                var changeLog = new ChangeLog
                {
                    EntityType = typeof(T).Name,
                    PropertyID = primaryKeyValue!,
                    PropertyName = property.Name,
                    OldValue = originalValue,
                    NewValue = updatedValue,
                    ChangedBy = changedBy,
                    ChangedAt = DateTime.UtcNow
                };
                changeLogs.Add(changeLog);
            }
        }

        return changeLogs;
    }
}
