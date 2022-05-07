#if TOOLS

using System.Collections.Generic;
using System.Linq;
using static Picalines.Godot.LDtkImport.Json.LevelJson;

namespace Picalines.Godot.LDtkImport.Json;

public static class FieldInstanceCollectionExtensions
{
    public static FieldInstance? FindField(this IEnumerable<FieldInstance> fieldInstances, string identifier, string? typeName)
    {
        return typeName is not null
            ? fieldInstances.FirstOrDefault(f => f.Identifier == identifier && f.Type == typeName)
            : fieldInstances.FirstOrDefault(f => f.Identifier == identifier);
    }

    public static T? GetValue<T>(this IEnumerable<FieldInstance> fieldInstances, string identifier, string? typeName = null)
    {
        var field = fieldInstances.FindField(identifier, typeName);
        return field is not null ? (T)field.Value : default;
    }

    public static bool TryGetValue<T>(this IEnumerable<FieldInstance> fieldInstances, string identifier, string? typeName, out T value)
    {
        var field = fieldInstances.FindField(identifier, typeName);

        if (field is not null)
        {
            value = (T)field.Value;
            return true;
        }

        value = default!;
        return false;
    }

    public static bool TryGetValue<T>(this IEnumerable<FieldInstance> fieldInstances, string identifier, out T value)
    {
        return fieldInstances.TryGetValue(identifier, null, out value);
    }
}

#endif
