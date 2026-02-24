using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering;

[NativeHeader("Runtime/Shaders/Keywords/KeywordSpaceScriptBindings.h")]
[NativeHeader("Runtime/Graphics/ShaderScriptBindings.h")]
[UsedByNativeCode]
public struct ShaderKeyword
{
	internal string m_Name;

	internal uint m_Index;

	internal bool m_IsLocal;

	internal bool m_IsCompute;

	internal bool m_IsValid;

	public string name => m_Name;

	public int index => (int)m_Index;

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ShaderScripting::GetGlobalKeywordCount")]
	internal static extern uint GetGlobalKeywordCount();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ShaderScripting::GetGlobalKeywordIndex")]
	internal static extern uint GetGlobalKeywordIndex(string keyword);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ShaderScripting::GetKeywordCount")]
	internal static extern uint GetKeywordCount(Shader shader);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ShaderScripting::GetKeywordIndex")]
	internal static extern uint GetKeywordIndex(Shader shader, string keyword);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ShaderScripting::GetKeywordCount")]
	internal static extern uint GetComputeShaderKeywordCount(ComputeShader shader);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ShaderScripting::GetKeywordIndex")]
	internal static extern uint GetComputeShaderKeywordIndex(ComputeShader shader, string keyword);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ShaderScripting::CreateGlobalKeyword")]
	internal static extern void CreateGlobalKeyword(string keyword);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ShaderScripting::GetKeywordType")]
	internal static extern ShaderKeywordType GetGlobalShaderKeywordType(uint keyword);

	public static ShaderKeywordType GetGlobalKeywordType(ShaderKeyword index)
	{
		if (index.IsValid())
		{
			return GetGlobalShaderKeywordType(index.m_Index);
		}
		return ShaderKeywordType.UserDefined;
	}

	public ShaderKeyword(string keywordName)
	{
		m_Name = keywordName;
		m_Index = GetGlobalKeywordIndex(keywordName);
		if (m_Index >= GetGlobalKeywordCount())
		{
			CreateGlobalKeyword(keywordName);
			m_Index = GetGlobalKeywordIndex(keywordName);
		}
		m_IsValid = true;
		m_IsLocal = false;
		m_IsCompute = false;
	}

	public ShaderKeyword(Shader shader, string keywordName)
	{
		m_Name = keywordName;
		m_Index = GetKeywordIndex(shader, keywordName);
		m_IsValid = m_Index < GetKeywordCount(shader);
		m_IsLocal = true;
		m_IsCompute = false;
	}

	public ShaderKeyword(ComputeShader shader, string keywordName)
	{
		m_Name = keywordName;
		m_Index = GetComputeShaderKeywordIndex(shader, keywordName);
		m_IsValid = m_Index < GetComputeShaderKeywordCount(shader);
		m_IsLocal = true;
		m_IsCompute = true;
	}

	public static bool IsKeywordLocal(ShaderKeyword keyword)
	{
		return keyword.m_IsLocal;
	}

	public bool IsValid()
	{
		return m_IsValid;
	}

	public bool IsValid(ComputeShader shader)
	{
		return m_IsValid;
	}

	public bool IsValid(Shader shader)
	{
		return m_IsValid;
	}

	public override string ToString()
	{
		return m_Name;
	}

	[Obsolete("GetKeywordType is deprecated. Only global keywords can have a type. This method always returns ShaderKeywordType.UserDefined.")]
	public static ShaderKeywordType GetKeywordType(Shader shader, ShaderKeyword index)
	{
		return ShaderKeywordType.UserDefined;
	}

	[Obsolete("GetKeywordType is deprecated. Only global keywords can have a type. This method always returns ShaderKeywordType.UserDefined.")]
	public static ShaderKeywordType GetKeywordType(ComputeShader shader, ShaderKeyword index)
	{
		return ShaderKeywordType.UserDefined;
	}

	[Obsolete("GetGlobalKeywordName is deprecated. Use the ShaderKeyword.name property instead.")]
	public static string GetGlobalKeywordName(ShaderKeyword index)
	{
		return index.m_Name;
	}

	[Obsolete("GetKeywordName is deprecated. Use the ShaderKeyword.name property instead.")]
	public static string GetKeywordName(Shader shader, ShaderKeyword index)
	{
		return index.m_Name;
	}

	[Obsolete("GetKeywordName is deprecated. Use the ShaderKeyword.name property instead.")]
	public static string GetKeywordName(ComputeShader shader, ShaderKeyword index)
	{
		return index.m_Name;
	}

	[Obsolete("GetKeywordType is deprecated. Use ShaderKeyword.name instead.")]
	public ShaderKeywordType GetKeywordType()
	{
		return GetGlobalKeywordType(this);
	}

	[Obsolete("GetKeywordName is deprecated. Use ShaderKeyword.name instead.")]
	public string GetKeywordName()
	{
		return GetGlobalKeywordName(this);
	}

	[Obsolete("GetName() has been deprecated. Use ShaderKeyword.name instead.")]
	public string GetName()
	{
		return GetKeywordName();
	}
}
