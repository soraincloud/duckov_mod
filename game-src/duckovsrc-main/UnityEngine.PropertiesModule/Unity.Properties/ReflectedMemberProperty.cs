using System;
using System.Reflection;
using System.Reflection.Emit;
using Unity.Collections;

namespace Unity.Properties;

public class ReflectedMemberProperty<TContainer, TValue> : Property<TContainer, TValue>
{
	private delegate TValue GetStructValueAction(ref TContainer container);

	private delegate void SetStructValueAction(ref TContainer container, TValue value);

	private delegate TValue GetClassValueAction(TContainer container);

	private delegate void SetClassValueAction(TContainer container, TValue value);

	private readonly IMemberInfo m_Info;

	private readonly bool m_IsStructContainerType;

	private GetStructValueAction m_GetStructValueAction;

	private SetStructValueAction m_SetStructValueAction;

	private GetClassValueAction m_GetClassValueAction;

	private SetClassValueAction m_SetClassValueAction;

	public override string Name { get; }

	public override bool IsReadOnly { get; }

	public ReflectedMemberProperty(FieldInfo info, string name)
		: this((IMemberInfo)new FieldMember(info), name)
	{
	}

	public ReflectedMemberProperty(PropertyInfo info, string name)
		: this((IMemberInfo)new PropertyMember(info), name)
	{
	}

	internal ReflectedMemberProperty(IMemberInfo info, string name)
	{
		Name = name;
		m_Info = info;
		m_IsStructContainerType = TypeTraits<TContainer>.IsValueType;
		AddAttributes(info.GetCustomAttributes());
		bool flag = (IsReadOnly = m_Info.IsReadOnly || HasAttribute<ReadOnlyAttribute>());
		if (m_Info is FieldMember fieldMember)
		{
			FieldInfo fieldInfo = fieldMember.m_FieldInfo;
			DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, fieldInfo.FieldType, new Type[1] { m_IsStructContainerType ? fieldInfo.ReflectedType.MakeByRefType() : fieldInfo.ReflectedType }, restrictedSkipVisibility: true);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldfld, fieldInfo);
			iLGenerator.Emit(OpCodes.Ret);
			if (m_IsStructContainerType)
			{
				m_GetStructValueAction = (GetStructValueAction)dynamicMethod.CreateDelegate(typeof(GetStructValueAction));
			}
			else
			{
				m_GetClassValueAction = (GetClassValueAction)dynamicMethod.CreateDelegate(typeof(GetClassValueAction));
			}
			if (!flag)
			{
				dynamicMethod = new DynamicMethod(string.Empty, typeof(void), new Type[2]
				{
					m_IsStructContainerType ? fieldInfo.ReflectedType.MakeByRefType() : fieldInfo.ReflectedType,
					fieldInfo.FieldType
				}, restrictedSkipVisibility: true);
				iLGenerator = dynamicMethod.GetILGenerator();
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldarg_1);
				iLGenerator.Emit(OpCodes.Stfld, fieldInfo);
				iLGenerator.Emit(OpCodes.Ret);
				if (m_IsStructContainerType)
				{
					m_SetStructValueAction = (SetStructValueAction)dynamicMethod.CreateDelegate(typeof(SetStructValueAction));
				}
				else
				{
					m_SetClassValueAction = (SetClassValueAction)dynamicMethod.CreateDelegate(typeof(SetClassValueAction));
				}
			}
		}
		else
		{
			if (!(m_Info is PropertyMember propertyMember))
			{
				return;
			}
			if (m_IsStructContainerType)
			{
				MethodInfo getMethod = propertyMember.m_PropertyInfo.GetGetMethod(nonPublic: true);
				m_GetStructValueAction = (GetStructValueAction)Delegate.CreateDelegate(typeof(GetStructValueAction), getMethod);
				if (!flag)
				{
					MethodInfo setMethod = propertyMember.m_PropertyInfo.GetSetMethod(nonPublic: true);
					m_SetStructValueAction = (SetStructValueAction)Delegate.CreateDelegate(typeof(SetStructValueAction), setMethod);
				}
			}
			else
			{
				MethodInfo getMethod2 = propertyMember.m_PropertyInfo.GetGetMethod(nonPublic: true);
				m_GetClassValueAction = (GetClassValueAction)Delegate.CreateDelegate(typeof(GetClassValueAction), getMethod2);
				if (!flag)
				{
					MethodInfo setMethod2 = propertyMember.m_PropertyInfo.GetSetMethod(nonPublic: true);
					m_SetClassValueAction = (SetClassValueAction)Delegate.CreateDelegate(typeof(SetClassValueAction), setMethod2);
				}
			}
		}
	}

	public override TValue GetValue(ref TContainer container)
	{
		if (m_IsStructContainerType)
		{
			return (m_GetStructValueAction == null) ? ((TValue)m_Info.GetValue(container)) : m_GetStructValueAction(ref container);
		}
		return (m_GetClassValueAction == null) ? ((TValue)m_Info.GetValue(container)) : m_GetClassValueAction(container);
	}

	public override void SetValue(ref TContainer container, TValue value)
	{
		if (IsReadOnly)
		{
			throw new InvalidOperationException("Property is ReadOnly.");
		}
		if (m_IsStructContainerType)
		{
			if (m_SetStructValueAction == null)
			{
				object obj = container;
				m_Info.SetValue(obj, value);
				container = (TContainer)obj;
			}
			else
			{
				m_SetStructValueAction(ref container, value);
			}
		}
		else if (m_SetClassValueAction == null)
		{
			m_Info.SetValue(container, value);
		}
		else
		{
			m_SetClassValueAction(container, value);
		}
	}
}
