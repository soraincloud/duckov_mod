using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Rendering;

[NativeHeader("Runtime/Shaders/Keywords/KeywordSpaceScriptBindings.h")]
public readonly struct LocalKeywordSpace : IEquatable<LocalKeywordSpace>
{
	private readonly IntPtr m_KeywordSpace;

	public LocalKeyword[] keywords => GetKeywords();

	public string[] keywordNames => GetKeywordNames();

	public uint keywordCount => GetKeywordCount();

	[FreeFunction("keywords::GetKeywords", HasExplicitThis = true)]
	private LocalKeyword[] GetKeywords()
	{
		return GetKeywords_Injected(ref this);
	}

	[FreeFunction("keywords::GetKeywordNames", HasExplicitThis = true)]
	private string[] GetKeywordNames()
	{
		return GetKeywordNames_Injected(ref this);
	}

	[FreeFunction("keywords::GetKeywordCount", HasExplicitThis = true)]
	private uint GetKeywordCount()
	{
		return GetKeywordCount_Injected(ref this);
	}

	[FreeFunction("keywords::GetKeyword", HasExplicitThis = true)]
	private LocalKeyword GetKeyword(string name)
	{
		GetKeyword_Injected(ref this, name, out var ret);
		return ret;
	}

	public LocalKeyword FindKeyword(string name)
	{
		return GetKeyword(name);
	}

	public override bool Equals(object o)
	{
		return o is LocalKeywordSpace rhs && Equals(rhs);
	}

	public bool Equals(LocalKeywordSpace rhs)
	{
		return m_KeywordSpace == rhs.m_KeywordSpace;
	}

	public static bool operator ==(LocalKeywordSpace lhs, LocalKeywordSpace rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(LocalKeywordSpace lhs, LocalKeywordSpace rhs)
	{
		return !(lhs == rhs);
	}

	public override int GetHashCode()
	{
		return m_KeywordSpace.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern LocalKeyword[] GetKeywords_Injected(ref LocalKeywordSpace _unity_self);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string[] GetKeywordNames_Injected(ref LocalKeywordSpace _unity_self);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern uint GetKeywordCount_Injected(ref LocalKeywordSpace _unity_self);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void GetKeyword_Injected(ref LocalKeywordSpace _unity_self, string name, out LocalKeyword ret);
}
