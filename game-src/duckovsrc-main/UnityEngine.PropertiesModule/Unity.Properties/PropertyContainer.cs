using System;
using System.Collections.Generic;
using Unity.Properties.Internal;
using UnityEngine.Pool;

namespace Unity.Properties;

public static class PropertyContainer
{
	private class GetPropertyVisitor : PathVisitor
	{
		public static readonly ObjectPool<GetPropertyVisitor> Pool = new ObjectPool<GetPropertyVisitor>(() => new GetPropertyVisitor(), null, delegate(GetPropertyVisitor v)
		{
			v.Reset();
		});

		public IProperty Property;

		public override void Reset()
		{
			base.Reset();
			Property = null;
			base.ReadonlyVisit = true;
		}

		protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
		{
			Property = property;
		}
	}

	private class GetValueVisitor<TSrcValue> : PathVisitor
	{
		public static readonly ObjectPool<GetValueVisitor<TSrcValue>> Pool = new ObjectPool<GetValueVisitor<TSrcValue>>(() => new GetValueVisitor<TSrcValue>(), null, delegate(GetValueVisitor<TSrcValue> v)
		{
			v.Reset();
		});

		public TSrcValue Value;

		public override void Reset()
		{
			base.Reset();
			Value = default(TSrcValue);
			base.ReadonlyVisit = true;
		}

		protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
		{
			if (!TypeConversion.TryConvert<TValue, TSrcValue>(ref value, out Value))
			{
				base.ReturnCode = VisitReturnCode.InvalidCast;
			}
		}
	}

	private class ValueAtPathVisitor : PathVisitor
	{
		public static readonly ObjectPool<ValueAtPathVisitor> Pool = new ObjectPool<ValueAtPathVisitor>(() => new ValueAtPathVisitor(), null, delegate(ValueAtPathVisitor v)
		{
			v.Reset();
		});

		public IPropertyVisitor Visitor;

		public override void Reset()
		{
			base.Reset();
			Visitor = null;
			base.ReadonlyVisit = true;
		}

		protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
		{
			((IPropertyAccept<TContainer>)property).Accept(Visitor, ref container);
		}
	}

	private class ExistsAtPathVisitor : PathVisitor
	{
		public static readonly ObjectPool<ExistsAtPathVisitor> Pool = new ObjectPool<ExistsAtPathVisitor>(() => new ExistsAtPathVisitor(), null, delegate(ExistsAtPathVisitor v)
		{
			v.Reset();
		});

		public bool Exists;

		public override void Reset()
		{
			base.Reset();
			Exists = false;
			base.ReadonlyVisit = true;
		}

		protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
		{
			Exists = true;
		}
	}

	internal class SetValueVisitor<TSrcValue> : PathVisitor
	{
		public static readonly ObjectPool<SetValueVisitor<TSrcValue>> Pool = new ObjectPool<SetValueVisitor<TSrcValue>>(() => new SetValueVisitor<TSrcValue>(), null, delegate(SetValueVisitor<TSrcValue> v)
		{
			v.Reset();
		});

		public TSrcValue Value;

		public override void Reset()
		{
			base.Reset();
			Value = default(TSrcValue);
		}

		protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
		{
			TValue destination;
			if (property.IsReadOnly)
			{
				base.ReturnCode = VisitReturnCode.AccessViolation;
			}
			else if (TypeConversion.TryConvert<TSrcValue, TValue>(ref Value, out destination))
			{
				property.SetValue(ref container, destination);
			}
			else
			{
				base.ReturnCode = VisitReturnCode.InvalidCast;
			}
		}
	}

	public static void Accept<TContainer>(IPropertyBagVisitor visitor, TContainer container, VisitParameters parameters = default(VisitParameters))
	{
		VisitReturnCode returnCode = VisitReturnCode.Ok;
		try
		{
			if (TryAccept(visitor, ref container, out returnCode, parameters))
			{
				return;
			}
		}
		catch (Exception)
		{
			if ((parameters.IgnoreExceptions & VisitExceptionKind.Visitor) == 0)
			{
				throw;
			}
		}
		if ((parameters.IgnoreExceptions & VisitExceptionKind.Internal) == 0)
		{
			switch (returnCode)
			{
			case VisitReturnCode.Ok:
			case VisitReturnCode.InvalidContainerType:
				break;
			case VisitReturnCode.NullContainer:
				throw new ArgumentException("The given container was null. Visitation only works for valid non-null containers.");
			case VisitReturnCode.MissingPropertyBag:
				throw new MissingPropertyBagException(container.GetType());
			default:
				throw new Exception(string.Format("Unexpected {0}=[{1}]", "VisitReturnCode", returnCode));
			}
		}
	}

	public static void Accept<TContainer>(IPropertyBagVisitor visitor, ref TContainer container, VisitParameters parameters = default(VisitParameters))
	{
		VisitReturnCode returnCode = VisitReturnCode.Ok;
		try
		{
			if (TryAccept(visitor, ref container, out returnCode, parameters))
			{
				return;
			}
		}
		catch (Exception)
		{
			if ((parameters.IgnoreExceptions & VisitExceptionKind.Visitor) == 0)
			{
				throw;
			}
		}
		if ((parameters.IgnoreExceptions & VisitExceptionKind.Internal) == 0)
		{
			switch (returnCode)
			{
			case VisitReturnCode.Ok:
			case VisitReturnCode.InvalidContainerType:
				break;
			case VisitReturnCode.NullContainer:
				throw new ArgumentException("The given container was null. Visitation only works for valid non-null containers.");
			case VisitReturnCode.MissingPropertyBag:
				throw new MissingPropertyBagException(container.GetType());
			default:
				throw new Exception(string.Format("Unexpected {0}=[{1}]", "VisitReturnCode", returnCode));
			}
		}
	}

	public static bool TryAccept<TContainer>(IPropertyBagVisitor visitor, ref TContainer container, VisitParameters parameters = default(VisitParameters))
	{
		VisitReturnCode returnCode;
		return TryAccept(visitor, ref container, out returnCode, parameters);
	}

	public static bool TryAccept<TContainer>(IPropertyBagVisitor visitor, ref TContainer container, out VisitReturnCode returnCode, VisitParameters parameters = default(VisitParameters))
	{
		if (!TypeTraits<TContainer>.IsContainer)
		{
			returnCode = VisitReturnCode.InvalidContainerType;
			return false;
		}
		if (TypeTraits<TContainer>.CanBeNull && EqualityComparer<TContainer>.Default.Equals(container, default(TContainer)))
		{
			returnCode = VisitReturnCode.NullContainer;
			return false;
		}
		if (!TypeTraits<TContainer>.IsValueType && typeof(TContainer) != container.GetType())
		{
			if (!TypeTraits.IsContainer(container.GetType()))
			{
				returnCode = VisitReturnCode.InvalidContainerType;
				return false;
			}
			IPropertyBag propertyBag = PropertyBagStore.GetPropertyBag(container.GetType());
			if (propertyBag == null)
			{
				returnCode = VisitReturnCode.MissingPropertyBag;
				return false;
			}
			object container2 = container;
			propertyBag.Accept(visitor, ref container2);
			container = (TContainer)container2;
		}
		else
		{
			IPropertyBag<TContainer> propertyBag2 = PropertyBagStore.GetPropertyBag<TContainer>();
			if (propertyBag2 == null)
			{
				returnCode = VisitReturnCode.MissingPropertyBag;
				return false;
			}
			PropertyBag.AcceptWithSpecializedVisitor(propertyBag2, visitor, ref container);
		}
		returnCode = VisitReturnCode.Ok;
		return true;
	}

	public static void Accept<TContainer>(IPropertyVisitor visitor, ref TContainer container, in PropertyPath path, VisitParameters parameters = default(VisitParameters))
	{
		ValueAtPathVisitor valueAtPathVisitor = ValueAtPathVisitor.Pool.Get();
		try
		{
			valueAtPathVisitor.Path = path;
			valueAtPathVisitor.Visitor = visitor;
			Accept(valueAtPathVisitor, ref container, parameters);
			if ((parameters.IgnoreExceptions & VisitExceptionKind.Internal) == 0)
			{
				switch (valueAtPathVisitor.ReturnCode)
				{
				case VisitReturnCode.Ok:
					break;
				case VisitReturnCode.InvalidPath:
					throw new InvalidPathException($"Failed to Visit at Path=[{path}]");
				default:
					throw new Exception(string.Format("Unexpected {0}=[{1}]", "VisitReturnCode", valueAtPathVisitor.ReturnCode));
				}
			}
		}
		finally
		{
			ValueAtPathVisitor.Pool.Release(valueAtPathVisitor);
		}
	}

	public static bool TryAccept<TContainer>(IPropertyVisitor visitor, ref TContainer container, in PropertyPath path, out VisitReturnCode returnCode, VisitParameters parameters = default(VisitParameters))
	{
		ValueAtPathVisitor valueAtPathVisitor = ValueAtPathVisitor.Pool.Get();
		try
		{
			valueAtPathVisitor.Path = path;
			valueAtPathVisitor.Visitor = visitor;
			return TryAccept(valueAtPathVisitor, ref container, out returnCode, parameters);
		}
		finally
		{
			ValueAtPathVisitor.Pool.Release(valueAtPathVisitor);
		}
	}

	public static IProperty GetProperty<TContainer>(TContainer container, in PropertyPath path)
	{
		return GetProperty(ref container, in path);
	}

	public static IProperty GetProperty<TContainer>(ref TContainer container, in PropertyPath path)
	{
		if (TryGetProperty(ref container, in path, out var property, out var returnCode))
		{
			return property;
		}
		switch (returnCode)
		{
		case VisitReturnCode.NullContainer:
			throw new ArgumentNullException("container");
		case VisitReturnCode.InvalidContainerType:
			throw new InvalidContainerTypeException(container.GetType());
		case VisitReturnCode.MissingPropertyBag:
			throw new MissingPropertyBagException(container.GetType());
		case VisitReturnCode.InvalidPath:
			throw new ArgumentException($"Failed to get property for path=[{path}]");
		default:
			throw new Exception(string.Format("Unexpected {0}=[{1}]", "VisitReturnCode", returnCode));
		}
	}

	public static bool TryGetProperty<TContainer>(TContainer container, in PropertyPath path, out IProperty property)
	{
		VisitReturnCode returnCode;
		return TryGetProperty(ref container, in path, out property, out returnCode);
	}

	public static bool TryGetProperty<TContainer>(ref TContainer container, in PropertyPath path, out IProperty property)
	{
		VisitReturnCode returnCode;
		return TryGetProperty(ref container, in path, out property, out returnCode);
	}

	public static bool TryGetProperty<TContainer>(ref TContainer container, in PropertyPath path, out IProperty property, out VisitReturnCode returnCode)
	{
		GetPropertyVisitor getPropertyVisitor = GetPropertyVisitor.Pool.Get();
		try
		{
			getPropertyVisitor.Path = path;
			if (!TryAccept(getPropertyVisitor, ref container, out returnCode))
			{
				property = null;
				return false;
			}
			returnCode = getPropertyVisitor.ReturnCode;
			property = getPropertyVisitor.Property;
			return returnCode == VisitReturnCode.Ok;
		}
		finally
		{
			GetPropertyVisitor.Pool.Release(getPropertyVisitor);
		}
	}

	public static TValue GetValue<TContainer, TValue>(TContainer container, string name)
	{
		return GetValue<TContainer, TValue>(ref container, name);
	}

	public static TValue GetValue<TContainer, TValue>(ref TContainer container, string name)
	{
		return GetValue<TContainer, TValue>(ref container, new PropertyPath(name));
	}

	public static TValue GetValue<TContainer, TValue>(TContainer container, in PropertyPath path)
	{
		return GetValue<TContainer, TValue>(ref container, in path);
	}

	public static TValue GetValue<TContainer, TValue>(ref TContainer container, in PropertyPath path)
	{
		if (path.IsEmpty)
		{
			throw new InvalidPathException("The specified PropertyPath is empty.");
		}
		if (TryGetValue<TContainer, TValue>(ref container, in path, out var value, out var returnCode))
		{
			return value;
		}
		switch (returnCode)
		{
		case VisitReturnCode.NullContainer:
			throw new ArgumentNullException("container");
		case VisitReturnCode.InvalidContainerType:
			throw new InvalidContainerTypeException(container.GetType());
		case VisitReturnCode.MissingPropertyBag:
			throw new MissingPropertyBagException(container.GetType());
		case VisitReturnCode.InvalidCast:
			throw new InvalidCastException($"Failed to GetValue of Type=[{typeof(TValue).Name}] for property with path=[{path}]");
		case VisitReturnCode.InvalidPath:
			throw new InvalidPathException($"Failed to GetValue for property with Path=[{path}]");
		default:
			throw new Exception(string.Format("Unexpected {0}=[{1}]", "VisitReturnCode", returnCode));
		}
	}

	public static bool TryGetValue<TContainer, TValue>(TContainer container, string name, out TValue value)
	{
		return TryGetValue<TContainer, TValue>(ref container, name, out value);
	}

	public static bool TryGetValue<TContainer, TValue>(ref TContainer container, string name, out TValue value)
	{
		VisitReturnCode returnCode;
		return TryGetValue<TContainer, TValue>(ref container, new PropertyPath(name), out value, out returnCode);
	}

	public static bool TryGetValue<TContainer, TValue>(TContainer container, in PropertyPath path, out TValue value)
	{
		VisitReturnCode returnCode;
		return TryGetValue<TContainer, TValue>(ref container, in path, out value, out returnCode);
	}

	public static bool TryGetValue<TContainer, TValue>(ref TContainer container, in PropertyPath path, out TValue value)
	{
		VisitReturnCode returnCode;
		return TryGetValue<TContainer, TValue>(ref container, in path, out value, out returnCode);
	}

	public static bool TryGetValue<TContainer, TValue>(ref TContainer container, in PropertyPath path, out TValue value, out VisitReturnCode returnCode)
	{
		if (path.IsEmpty)
		{
			returnCode = VisitReturnCode.InvalidPath;
			value = default(TValue);
			return false;
		}
		GetValueVisitor<TValue> getValueVisitor = GetValueVisitor<TValue>.Pool.Get();
		getValueVisitor.Path = path;
		getValueVisitor.ReadonlyVisit = true;
		try
		{
			if (!TryAccept(getValueVisitor, ref container, out returnCode))
			{
				value = default(TValue);
				return false;
			}
			value = getValueVisitor.Value;
			returnCode = getValueVisitor.ReturnCode;
		}
		finally
		{
			GetValueVisitor<TValue>.Pool.Release(getValueVisitor);
		}
		return returnCode == VisitReturnCode.Ok;
	}

	public static bool IsPathValid<TContainer>(TContainer container, string path)
	{
		return IsPathValid(ref container, new PropertyPath(path));
	}

	public static bool IsPathValid<TContainer>(TContainer container, in PropertyPath path)
	{
		return IsPathValid(ref container, in path);
	}

	public static bool IsPathValid<TContainer>(ref TContainer container, string path)
	{
		ExistsAtPathVisitor existsAtPathVisitor = ExistsAtPathVisitor.Pool.Get();
		try
		{
			existsAtPathVisitor.Path = new PropertyPath(path);
			TryAccept(existsAtPathVisitor, ref container);
			return existsAtPathVisitor.Exists;
		}
		finally
		{
			ExistsAtPathVisitor.Pool.Release(existsAtPathVisitor);
		}
	}

	public static bool IsPathValid<TContainer>(ref TContainer container, in PropertyPath path)
	{
		ExistsAtPathVisitor existsAtPathVisitor = ExistsAtPathVisitor.Pool.Get();
		try
		{
			existsAtPathVisitor.Path = path;
			TryAccept(existsAtPathVisitor, ref container);
			return existsAtPathVisitor.Exists;
		}
		finally
		{
			ExistsAtPathVisitor.Pool.Release(existsAtPathVisitor);
		}
	}

	public static void SetValue<TContainer, TValue>(TContainer container, string name, TValue value)
	{
		SetValue(ref container, name, value);
	}

	public static void SetValue<TContainer, TValue>(ref TContainer container, string name, TValue value)
	{
		SetValue(ref container, new PropertyPath(name), value);
	}

	public static void SetValue<TContainer, TValue>(TContainer container, in PropertyPath path, TValue value)
	{
		SetValue(ref container, in path, value);
	}

	public static void SetValue<TContainer, TValue>(ref TContainer container, in PropertyPath path, TValue value)
	{
		if (path.Length == 0)
		{
			throw new ArgumentNullException("path");
		}
		if (path.Length <= 0)
		{
			throw new InvalidPathException("The specified PropertyPath is empty.");
		}
		if (TrySetValue(ref container, in path, value, out var returnCode))
		{
			return;
		}
		switch (returnCode)
		{
		case VisitReturnCode.NullContainer:
			throw new ArgumentNullException("container");
		case VisitReturnCode.InvalidContainerType:
			throw new InvalidContainerTypeException(container.GetType());
		case VisitReturnCode.MissingPropertyBag:
			throw new MissingPropertyBagException(container.GetType());
		case VisitReturnCode.InvalidCast:
			throw new InvalidCastException($"Failed to SetValue of Type=[{typeof(TValue).Name}] for property with path=[{path}]");
		case VisitReturnCode.InvalidPath:
			throw new InvalidPathException($"Failed to SetValue for property with Path=[{path}]");
		case VisitReturnCode.AccessViolation:
			throw new AccessViolationException($"Failed to SetValue for read-only property with Path=[{path}]");
		default:
			throw new Exception(string.Format("Unexpected {0}=[{1}]", "VisitReturnCode", returnCode));
		}
	}

	public static bool TrySetValue<TContainer, TValue>(TContainer container, string name, TValue value)
	{
		return TrySetValue(ref container, name, value);
	}

	public static bool TrySetValue<TContainer, TValue>(ref TContainer container, string name, TValue value)
	{
		return TrySetValue(ref container, new PropertyPath(name), value);
	}

	public static bool TrySetValue<TContainer, TValue>(TContainer container, in PropertyPath path, TValue value)
	{
		return TrySetValue(ref container, in path, value);
	}

	public static bool TrySetValue<TContainer, TValue>(ref TContainer container, in PropertyPath path, TValue value)
	{
		VisitReturnCode returnCode;
		return TrySetValue(ref container, in path, value, out returnCode);
	}

	public static bool TrySetValue<TContainer, TValue>(ref TContainer container, in PropertyPath path, TValue value, out VisitReturnCode returnCode)
	{
		if (path.IsEmpty)
		{
			returnCode = VisitReturnCode.InvalidPath;
			return false;
		}
		SetValueVisitor<TValue> setValueVisitor = SetValueVisitor<TValue>.Pool.Get();
		setValueVisitor.Path = path;
		setValueVisitor.Value = value;
		try
		{
			if (!TryAccept(setValueVisitor, ref container, out returnCode))
			{
				return false;
			}
			returnCode = setValueVisitor.ReturnCode;
		}
		finally
		{
			SetValueVisitor<TValue>.Pool.Release(setValueVisitor);
		}
		return returnCode == VisitReturnCode.Ok;
	}
}
