using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering;

[NativeHeader("Runtime/Shaders/PassIdentifier.h")]
[UsedByNativeCode]
public readonly struct PassIdentifier : IEquatable<PassIdentifier>
{
	internal readonly uint m_SubShaderIndex;

	internal readonly uint m_PassIndex;

	public uint SubshaderIndex => m_SubShaderIndex;

	public uint PassIndex => m_PassIndex;

	public PassIdentifier(uint subshaderIndex, uint passIndex)
	{
		m_SubShaderIndex = subshaderIndex;
		m_PassIndex = passIndex;
	}

	public override bool Equals(object o)
	{
		return o is PassIdentifier rhs && Equals(rhs);
	}

	public bool Equals(PassIdentifier rhs)
	{
		return m_SubShaderIndex == rhs.m_SubShaderIndex && m_PassIndex == rhs.m_PassIndex;
	}

	public static bool operator ==(PassIdentifier lhs, PassIdentifier rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(PassIdentifier lhs, PassIdentifier rhs)
	{
		return !(lhs == rhs);
	}

	public override int GetHashCode()
	{
		return m_SubShaderIndex.GetHashCode() ^ m_PassIndex.GetHashCode();
	}
}
