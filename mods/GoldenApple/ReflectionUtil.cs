using System;
using System.Reflection;

namespace GoldenApple;

internal static class ReflectionUtil
{
    public static void SetPrivateField<TTarget>(TTarget target, string fieldName, object? value)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));

        var type = target.GetType();
        var field = FindField(type, fieldName);
        if (field == null)
        {
            throw new MissingFieldException(type.FullName, fieldName);
        }

        field.SetValue(target, value);
    }

    public static TField? GetPrivateField<TField>(object target, string fieldName) where TField : class
    {
        if (target == null) throw new ArgumentNullException(nameof(target));

        var type = target.GetType();
        var field = FindField(type, fieldName);
        if (field == null)
        {
            return null;
        }

        return field.GetValue(target) as TField;
    }

    private static FieldInfo? FindField(Type? type, string fieldName)
    {
        while (type != null)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                return field;
            }

            type = type.BaseType;
        }

        return null;
    }
}