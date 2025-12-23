using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FilmManagement.UI.CA.Extensions;

public static class EnumExtensions
{
    public static T? ParseEnum<T>(this string value, bool ignoreCase = true) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        value = value.Trim();
        
        if (Enum.TryParse<T>(value, ignoreCase, out var result))
            return result;
        
        if (int.TryParse(value, out int intValue) && Enum.IsDefined(typeof(T), intValue))
            return (T)(object)intValue;
        
        return null;
    }
    
    public static string GetDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var displayAttribute = field?
            .GetCustomAttribute<DisplayAttribute>();

        return displayAttribute?.Name ?? value.ToString();
    }
    
    public static IEnumerable<string> GetAllDisplayNames<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<Enum>()
            .Select(e => e.GetDisplayName());
    }
    
    public static string GetEnumOptionsPrompt<T>() where T : struct, Enum
    {
        var options = Enum.GetValues(typeof(T))
            .Cast<Enum>()
            .Select(e => $"{Convert.ToInt32(e)}={e.GetDisplayName()}");
        
        return string.Join(", ", options);
    }
    
    public static T ParseEnumFlags<T>(this string value, bool ignoreCase = true) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return default;
    
        int result = 0;
        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
    
        foreach (var part in parts)
        {
            var parsed = part.Trim().ParseEnum<T>(ignoreCase);
            if (parsed.HasValue)
            {
                result |= Convert.ToInt32(parsed.Value);
            }
        }
        
        return (T)Enum.ToObject(typeof(T), result);
    }
}