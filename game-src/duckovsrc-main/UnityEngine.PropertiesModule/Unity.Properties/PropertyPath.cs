using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine.Pool;

namespace Unity.Properties;

public readonly struct PropertyPath : IEquatable<PropertyPath>
{
	internal const int k_InlineCount = 4;

	private readonly PropertyPathPart m_Part0;

	private readonly PropertyPathPart m_Part1;

	private readonly PropertyPathPart m_Part2;

	private readonly PropertyPathPart m_Part3;

	private readonly int m_InlinePartsCount;

	private readonly PropertyPathPart[] m_AdditionalParts;

	public int Length { get; }

	public bool IsEmpty => Length == 0;

	public PropertyPathPart this[int index]
	{
		get
		{
			switch (index)
			{
			case 0:
				if (Length < 1)
				{
					throw new IndexOutOfRangeException();
				}
				return m_Part0;
			case 1:
				if (Length < 2)
				{
					throw new IndexOutOfRangeException();
				}
				return m_Part1;
			case 2:
				if (Length < 3)
				{
					throw new IndexOutOfRangeException();
				}
				return m_Part2;
			case 3:
				if (Length < 4)
				{
					throw new IndexOutOfRangeException();
				}
				return m_Part3;
			default:
				return m_AdditionalParts[index - 4];
			}
		}
	}

	public PropertyPath(string path)
	{
		PropertyPath propertyPath = ConstructFromPath(path);
		m_Part0 = propertyPath.m_Part0;
		m_Part1 = propertyPath.m_Part1;
		m_Part2 = propertyPath.m_Part2;
		m_Part3 = propertyPath.m_Part3;
		m_AdditionalParts = propertyPath.m_AdditionalParts;
		m_InlinePartsCount = propertyPath.m_InlinePartsCount;
		int inlinePartsCount = m_InlinePartsCount;
		PropertyPathPart[] additionalParts = m_AdditionalParts;
		Length = inlinePartsCount + ((additionalParts != null) ? additionalParts.Length : 0);
	}

	private PropertyPath(in PropertyPathPart part)
	{
		m_Part0 = part;
		m_Part1 = default(PropertyPathPart);
		m_Part2 = default(PropertyPathPart);
		m_Part3 = default(PropertyPathPart);
		m_AdditionalParts = null;
		m_InlinePartsCount = 1;
		Length = 1;
	}

	private PropertyPath(in PropertyPathPart part0, in PropertyPathPart part1)
	{
		m_Part0 = part0;
		m_Part1 = part1;
		m_Part2 = default(PropertyPathPart);
		m_Part3 = default(PropertyPathPart);
		m_AdditionalParts = null;
		m_InlinePartsCount = 2;
		Length = 2;
	}

	private PropertyPath(in PropertyPathPart part0, in PropertyPathPart part1, in PropertyPathPart part2)
	{
		m_Part0 = part0;
		m_Part1 = part1;
		m_Part2 = part2;
		m_Part3 = default(PropertyPathPart);
		m_AdditionalParts = null;
		m_InlinePartsCount = 3;
		Length = 3;
	}

	private PropertyPath(in PropertyPathPart part0, in PropertyPathPart part1, in PropertyPathPart part2, in PropertyPathPart part3)
	{
		m_Part0 = part0;
		m_Part1 = part1;
		m_Part2 = part2;
		m_Part3 = part3;
		m_AdditionalParts = null;
		m_InlinePartsCount = 4;
		Length = 4;
	}

	internal PropertyPath(List<PropertyPathPart> parts)
	{
		m_Part0 = default(PropertyPathPart);
		m_Part1 = default(PropertyPathPart);
		m_Part2 = default(PropertyPathPart);
		m_Part3 = default(PropertyPathPart);
		m_InlinePartsCount = 0;
		m_AdditionalParts = ((parts.Count > 4) ? new PropertyPathPart[parts.Count - 4] : null);
		for (int i = 0; i < parts.Count; i++)
		{
			switch (i)
			{
			case 0:
				m_Part0 = parts[i];
				m_InlinePartsCount++;
				break;
			case 1:
				m_Part1 = parts[i];
				m_InlinePartsCount++;
				break;
			case 2:
				m_Part2 = parts[i];
				m_InlinePartsCount++;
				break;
			case 3:
				m_Part3 = parts[i];
				m_InlinePartsCount++;
				break;
			default:
				m_AdditionalParts[i - 4] = parts[i];
				break;
			}
		}
		Length = parts.Count;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyPath FromPart(in PropertyPathPart part)
	{
		return new PropertyPath(in part);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyPath FromName(string name)
	{
		return new PropertyPath(new PropertyPathPart(name));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyPath FromIndex(int index)
	{
		return new PropertyPath(new PropertyPathPart(index));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyPath FromKey(object key)
	{
		return new PropertyPath(new PropertyPathPart(key));
	}

	public static PropertyPath Combine(in PropertyPath path, in PropertyPath pathToAppend)
	{
		if (path.IsEmpty)
		{
			return pathToAppend;
		}
		if (pathToAppend.IsEmpty)
		{
			return path;
		}
		int length = path.Length;
		int length2 = pathToAppend.Length;
		int num = length + length2;
		if (num <= 4)
		{
			int index = 0;
			PropertyPathPart part = path[0];
			PropertyPathPart part2 = ((length > 1) ? path[1] : pathToAppend[index++]);
			PropertyPathPart part3 = ((num <= 2) ? default(PropertyPathPart) : ((length > 2) ? path[2] : pathToAppend[index++]));
			PropertyPathPart part4 = ((num <= 3) ? default(PropertyPathPart) : ((length > 3) ? path[3] : pathToAppend[index]));
			switch (num)
			{
			case 2:
				return new PropertyPath(in part, in part2);
			case 3:
				return new PropertyPath(in part, in part2, in part3);
			case 4:
				return new PropertyPath(in part, in part2, in part3, in part4);
			}
		}
		List<PropertyPathPart> list = CollectionPool<List<PropertyPathPart>, PropertyPathPart>.Get();
		try
		{
			GetParts(in path, list);
			GetParts(in pathToAppend, list);
			return new PropertyPath(list);
		}
		finally
		{
			CollectionPool<List<PropertyPathPart>, PropertyPathPart>.Release(list);
		}
	}

	public static PropertyPath Combine(in PropertyPath path, string pathToAppend)
	{
		if (string.IsNullOrEmpty(pathToAppend))
		{
			return path;
		}
		return Combine(in path, new PropertyPath(pathToAppend));
	}

	public static PropertyPath AppendPart(in PropertyPath path, in PropertyPathPart part)
	{
		if (path.IsEmpty)
		{
			return new PropertyPath(in part);
		}
		switch (path.Length)
		{
		case 1:
			return new PropertyPath(path[0], in part);
		case 2:
			return new PropertyPath(path[0], path[1], in part);
		case 3:
			return new PropertyPath(path[0], path[1], path[2], in part);
		default:
		{
			List<PropertyPathPart> list = CollectionPool<List<PropertyPathPart>, PropertyPathPart>.Get();
			try
			{
				GetParts(in path, list);
				list.Add(part);
				return new PropertyPath(list);
			}
			finally
			{
				CollectionPool<List<PropertyPathPart>, PropertyPathPart>.Release(list);
			}
		}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyPath AppendName(in PropertyPath path, string name)
	{
		return AppendPart(in path, new PropertyPathPart(name));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyPath AppendIndex(in PropertyPath path, int index)
	{
		return AppendPart(in path, new PropertyPathPart(index));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyPath AppendKey(in PropertyPath path, object key)
	{
		return AppendPart(in path, new PropertyPathPart(key));
	}

	public static PropertyPath AppendProperty(in PropertyPath path, IProperty property)
	{
		if (1 == 0)
		{
		}
		PropertyPath result = ((property is IListElementProperty listElementProperty) ? AppendPart(in path, new PropertyPathPart(listElementProperty.Index)) : ((property is ISetElementProperty setElementProperty) ? AppendPart(in path, new PropertyPathPart(setElementProperty.ObjectKey)) : ((!(property is IDictionaryElementProperty dictionaryElementProperty)) ? AppendPart(in path, new PropertyPathPart(property.Name)) : AppendPart(in path, new PropertyPathPart(dictionaryElementProperty.ObjectKey)))));
		if (1 == 0)
		{
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyPath Pop(in PropertyPath path)
	{
		return SubPath(in path, 0, path.Length - 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyPath SubPath(in PropertyPath path, int startIndex)
	{
		return SubPath(in path, startIndex, path.Length - startIndex);
	}

	public static PropertyPath SubPath(in PropertyPath path, int startIndex, int length)
	{
		int length2 = path.Length;
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		if (startIndex > length2)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (startIndex > length2 - length)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (length == 0)
		{
			return default(PropertyPath);
		}
		if (startIndex == 0 && length == length2)
		{
			return path;
		}
		switch (length)
		{
		case 1:
			return new PropertyPath(path[startIndex]);
		case 2:
			return new PropertyPath(path[startIndex], path[startIndex + 1]);
		case 3:
			return new PropertyPath(path[startIndex], path[startIndex + 1], path[startIndex + 2]);
		case 4:
			return new PropertyPath(path[startIndex], path[startIndex + 1], path[startIndex + 2], path[startIndex + 3]);
		default:
		{
			List<PropertyPathPart> list = CollectionPool<List<PropertyPathPart>, PropertyPathPart>.Get();
			try
			{
				for (int i = startIndex; i < startIndex + length; i++)
				{
					list.Add(path[i]);
				}
				return new PropertyPath(list);
			}
			finally
			{
				CollectionPool<List<PropertyPathPart>, PropertyPathPart>.Release(list);
			}
		}
		}
	}

	public override string ToString()
	{
		if (Length == 0)
		{
			return string.Empty;
		}
		if (Length == 1 && m_Part0.IsName)
		{
			return m_Part0.Name;
		}
		StringBuilder stringBuilder = new StringBuilder(32);
		if (Length > 0)
		{
			AppendToBuilder(in m_Part0, stringBuilder);
		}
		if (Length > 1)
		{
			AppendToBuilder(in m_Part1, stringBuilder);
		}
		if (Length > 2)
		{
			AppendToBuilder(in m_Part2, stringBuilder);
		}
		if (Length > 3)
		{
			AppendToBuilder(in m_Part3, stringBuilder);
		}
		if (Length > 4)
		{
			PropertyPathPart[] additionalParts = m_AdditionalParts;
			for (int i = 0; i < additionalParts.Length; i++)
			{
				PropertyPathPart part = additionalParts[i];
				AppendToBuilder(in part, stringBuilder);
			}
		}
		return stringBuilder.ToString();
	}

	private static void AppendToBuilder(in PropertyPathPart part, StringBuilder builder)
	{
		switch (part.Kind)
		{
		case PropertyPathPartKind.Name:
			if (builder.Length > 0)
			{
				builder.Append('.');
			}
			builder.Append(part.ToString());
			break;
		case PropertyPathPartKind.Index:
		case PropertyPathPartKind.Key:
			builder.Append(part.ToString());
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private static void GetParts(in PropertyPath path, List<PropertyPathPart> parts)
	{
		int length = path.Length;
		for (int i = 0; i < length; i++)
		{
			parts.Add(path[i]);
		}
	}

	private static PropertyPath ConstructFromPath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return default(PropertyPath);
		}
		int index = 0;
		int length = path.Length;
		int state = 0;
		List<PropertyPathPart> list = CollectionPool<List<PropertyPathPart>, PropertyPathPart>.Get();
		try
		{
			list.Clear();
			while (index < length)
			{
				switch (state)
				{
				case 0:
					TrimStart();
					if (index == length)
					{
						break;
					}
					if (path[index] == '.')
					{
						throw new ArgumentException(string.Format("{0}: Invalid '{1}' character encountered at index '{2}'.", "PropertyPath", path[index], index));
					}
					if (path[index] == '[')
					{
						state = 2;
						break;
					}
					if (path[index] == '"')
					{
						throw new ArgumentException(string.Format("{0}: Invalid '{1}' character encountered at index '{2}'.", "PropertyPath", path[index], index));
					}
					state = 1;
					break;
				case 1:
				{
					int num3 = index;
					while (index < length && path[index] != '.' && path[index] != '[')
					{
						int num = index + 1;
						index = num;
					}
					if (num3 == index)
					{
						throw new ArgumentException("Invalid PropertyPath: Name is empty.");
					}
					if (index == length)
					{
						list.Add(new PropertyPathPart(path.Substring(num3)));
						state = 0;
					}
					else
					{
						list.Add(new PropertyPathPart(path.Substring(num3, index - num3)));
						ReadNext();
					}
					break;
				}
				case 2:
					if (path[index] != '[')
					{
						throw new ArgumentException(string.Format("{0}: Invalid '{1}' character encountered at index '{2}'.", "PropertyPath", path[index], index));
					}
					if (index + 1 < length && path[index + 1] == '"')
					{
						state = 4;
					}
					else
					{
						state = 3;
					}
					break;
				case 3:
				{
					if (path[index] != '[')
					{
						throw new ArgumentException(string.Format("{0}: Invalid '{1}' character encountered at index '{2}'.", "PropertyPath", path[index], index));
					}
					int num = index + 1;
					index = num;
					int num4 = index;
					while (index < length)
					{
						char c2 = path[index];
						if (c2 == ']')
						{
							break;
						}
						num = index + 1;
						index = num;
					}
					if (path[index] != ']')
					{
						throw new ArgumentException(string.Format("{0}: Invalid '{1}' character encountered at index '{2}'.", "PropertyPath", path[index], index));
					}
					string s = path.Substring(num4, index - num4);
					if (!int.TryParse(s, out var result))
					{
						throw new ArgumentException("Indices in PropertyPath must be a numeric value.");
					}
					if (result < 0)
					{
						throw new ArgumentException("Invalid PropertyPath: Negative indices are not supported.");
					}
					list.Add(new PropertyPathPart(result));
					num = index + 1;
					index = num;
					if (index != length)
					{
						ReadNext();
					}
					break;
				}
				case 4:
				{
					if (path[index] != '[')
					{
						throw new ArgumentException(string.Format("{0}: Invalid '{1}' character encountered at index '{2}'.", "PropertyPath", path[index], index));
					}
					int num = index + 1;
					index = num;
					if (path[index] != '"')
					{
						throw new ArgumentException(string.Format("{0}: Invalid '{1}' character encountered at index '{2}'.", "PropertyPath", path[index], index));
					}
					num = index + 1;
					index = num;
					int num2 = index;
					while (index < length)
					{
						char c = path[index];
						if (c == '"')
						{
							break;
						}
						num = index + 1;
						index = num;
					}
					if (path[index] != '"')
					{
						throw new ArgumentException(string.Format("{0}: Invalid '{1}' character encountered at index '{2}'.", "PropertyPath", path[index], index));
					}
					if (index + 1 < length && path[index + 1] == ']')
					{
						string key = path.Substring(num2, index - num2);
						list.Add(new PropertyPathPart((object)key));
						index += 2;
						ReadNext();
						break;
					}
					throw new ArgumentException("Invalid PropertyPath: No matching end quote for key.");
				}
				}
			}
			return new PropertyPath(list);
		}
		finally
		{
			CollectionPool<List<PropertyPathPart>, PropertyPathPart>.Release(list);
		}
		void ReadNext()
		{
			if (index == length)
			{
				state = 0;
			}
			else
			{
				switch (path[index])
				{
				case '.':
				{
					int num5 = index + 1;
					index = num5;
					state = 0;
					break;
				}
				case '[':
					state = 2;
					break;
				default:
					throw new ArgumentException(string.Format("{0}: Invalid '{1}' character encountered at index '{2}'.", "PropertyPath", path[index], index));
				}
			}
		}
		void TrimStart()
		{
			while (index < length && path[index] == ' ')
			{
				int num5 = index + 1;
				index = num5;
			}
		}
	}

	public bool Equals(PropertyPath other)
	{
		if (Length != other.Length)
		{
			return false;
		}
		for (int i = 0; i < Length; i++)
		{
			if (!this[i].Equals(other[i]))
			{
				return false;
			}
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is PropertyPath other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = 19;
		int length = Length;
		if (length == 0)
		{
			return num;
		}
		if (length > 0)
		{
			num = num * 31 + m_Part0.GetHashCode();
		}
		if (length > 1)
		{
			num = num * 31 + m_Part1.GetHashCode();
		}
		if (length > 2)
		{
			num = num * 31 + m_Part2.GetHashCode();
		}
		if (length > 3)
		{
			num = num * 31 + m_Part3.GetHashCode();
		}
		if (length <= 4)
		{
			return num;
		}
		PropertyPathPart[] additionalParts = m_AdditionalParts;
		for (int i = 0; i < additionalParts.Length; i++)
		{
			PropertyPathPart propertyPathPart = additionalParts[i];
			num = num * 31 + propertyPathPart.GetHashCode();
		}
		return num;
	}
}
