using System;
using System.Reflection;

namespace EnderPearl;

internal static class ReflectionUtil
{
    public static void SetPrivateField<TTarget>(TTarget target, string fieldName, object? value)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));

        var type = target!.GetType();
        var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
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
        var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
        {
            return null;
        }

        return field.GetValue(target) as TField;
    }
}
