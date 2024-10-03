using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;

namespace Apartments.Application.Utilities;

public static class CoreUtilities
{
    private static readonly List<string> DefaultExcludedProperties = new()
    {
        "ID", "Id", "User", "Apartment", "Tenant", "CreatedDate"
    };

    public static T ValidateEnum<T>(string type) where T : struct, Enum
    {
        if (!Enum.TryParse<T>(type, true, out var result))
            throw new BadRequestException(
                $"Invalid type. Allowed parameters are: {string.Join(", ", Enum.GetNames(typeof(T)))}");

        return result;
    }

    public static List<ChangeLog> GenerateChangeLogs<T>(T original, T updated, string changedBy,
        string primaryKeyValue, string[]? additionalPropertiesToExclude = null)
    {
        var propertiesToExclude = GetExcludedProperties(additionalPropertiesToExclude);
        return CompareEntities(original, updated, changedBy, primaryKeyValue, propertiesToExclude);
    }

    public static List<ChangeLog> GenerateChangeLogs<T>(List<T> originalList, List<T> updatedList,
        string changedBy, string[]? additionalPropertiesToExclude = null) where T : class
    {
        if (originalList.Count != updatedList.Count)
            throw new ArgumentException("The original and updated lists must have the same length.");

        var propertiesToExclude = GetExcludedProperties(additionalPropertiesToExclude);
        var changeLogs = new List<ChangeLog>();

        for (var i = 0; i < originalList.Count; i++)
        {
            var original = originalList[i];
            var updated = updatedList[i];

            var primaryKeyValue = typeof(T).GetProperty("Id")?.GetValue(original)?.ToString();

            if (primaryKeyValue == null)
                throw new InvalidOperationException(
                    $"Entity of type {typeof(T).Name} does not have a valid 'Id' property.");

            changeLogs.AddRange(CompareEntities(original, updated, changedBy, primaryKeyValue, propertiesToExclude));
        }

        return changeLogs;
    }

    private static List<string> GetExcludedProperties(string[]? additionalPropertiesToExclude)
    {
        var propertiesToExclude = new List<string>(DefaultExcludedProperties);
        if (additionalPropertiesToExclude != null) propertiesToExclude.AddRange(additionalPropertiesToExclude);
        return propertiesToExclude;
    }

    private static List<ChangeLog> CompareEntities<T>(T original, T updated, string changedBy,
        string primaryKeyValue, List<string> propertiesToExclude)
    {
        var changeLogs = new List<ChangeLog>();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            if (propertiesToExclude.Contains(property.Name)) continue;

            var originalValue = property.GetValue(original)?.ToString();
            var updatedValue = property.GetValue(updated)?.ToString();

            if (originalValue != updatedValue)
                changeLogs.Add(new ChangeLog
                {
                    EntityType = typeof(T).Name,
                    PropertyId = primaryKeyValue,
                    PropertyName = property.Name,
                    OldValue = originalValue,
                    NewValue = updatedValue,
                    ChangedBy = changedBy,
                    ChangedAt = DateTime.UtcNow
                });
        }

        return changeLogs;
    }
    public static string ConstructUserFullName(string firstName, string lastName)
    {
        if(string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName)) 
            return string.Empty;

        return $"{firstName}, {lastName}";
    }
    public static string NormalizeEmail(string email)
    {
        if (email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase) ||
            email.EndsWith("@googlemail.com", StringComparison.OrdinalIgnoreCase))
        {
            var atIndex = email.IndexOf('@');
            var username = email.Substring(0, atIndex).Replace(".", string.Empty);
            var domain = email.Substring(atIndex);

            return username + domain;
        }

        return email;
    }
}