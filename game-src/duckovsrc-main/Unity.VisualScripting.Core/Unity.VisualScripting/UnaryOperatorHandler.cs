using System;
using System.Collections.Generic;
using System.Reflection;

namespace Unity.VisualScripting;

public abstract class UnaryOperatorHandler : OperatorHandler
{
	private readonly Dictionary<Type, Func<object, object>> manualHandlers = new Dictionary<Type, Func<object, object>>();

	private readonly Dictionary<Type, IOptimizedInvoker> userDefinedOperators = new Dictionary<Type, IOptimizedInvoker>();

	private readonly Dictionary<Type, Type> userDefinedOperandTypes = new Dictionary<Type, Type>();

	protected UnaryOperatorHandler(string name, string verb, string symbol, string customMethodName)
		: base(name, verb, symbol, customMethodName)
	{
	}

	public object Operate(object operand)
	{
		Ensure.That("operand").IsNotNull(operand);
		Type type = operand.GetType();
		if (manualHandlers.ContainsKey(type))
		{
			return manualHandlers[type](operand);
		}
		if (base.customMethodName != null)
		{
			if (!userDefinedOperators.ContainsKey(type))
			{
				MethodInfo method = type.GetMethod(base.customMethodName, BindingFlags.Static | BindingFlags.Public);
				if (method != null)
				{
					userDefinedOperandTypes.Add(type, ResolveUserDefinedOperandType(method));
				}
				userDefinedOperators.Add(type, method?.Prewarm());
			}
			if (userDefinedOperators[type] != null)
			{
				operand = ConversionUtility.Convert(operand, userDefinedOperandTypes[type]);
				return userDefinedOperators[type].Invoke(null, operand);
			}
		}
		return CustomHandling(operand);
	}

	protected virtual object CustomHandling(object operand)
	{
		throw new InvalidOperatorException(base.symbol, operand.GetType());
	}

	protected void Handle<T>(Func<T, object> handler)
	{
		manualHandlers.Add(typeof(T), (object operand) => handler((T)operand));
	}

	private static Type ResolveUserDefinedOperandType(MethodInfo userDefinedOperator)
	{
		return userDefinedOperator.GetParameters()[0].ParameterType;
	}
}
