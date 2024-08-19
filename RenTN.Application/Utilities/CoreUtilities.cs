using RenTN.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace RenTN.Application.Utilities;

public static class CoreUtilities
{
    public static List<ChangeLog> GenerateChangeLogs<T>(T original, T updated, string changedBy, string primaryKeyValue, List<string>? additionalPropertiesToExclude = null)
    {
        var propertiesToExclude = new List<string>() { "ID", "Id", "User", "Apartment", "Tenant" };

        if (additionalPropertiesToExclude != null)
        {
            propertiesToExclude.AddRange(additionalPropertiesToExclude);
        }

        var changeLogs = new List<ChangeLog>();

        var properties = typeof(T).GetProperties();
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
                    PropertyID = primaryKeyValue,
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
