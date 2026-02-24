using UnityEngine.Scripting;

namespace UnityEngine.Rendering;

[UsedByNativeCode]
public struct PlatformKeywordSet
{
	private const int k_SizeInBits = 64;

	internal ulong m_Bits;

	private ulong ComputeKeywordMask(BuiltinShaderDefine define)
	{
		return (ulong)(1 << (int)define % 64);
	}

	public bool IsEnabled(BuiltinShaderDefine define)
	{
		return (m_Bits & ComputeKeywordMask(define)) != 0;
	}

	public void Enable(BuiltinShaderDefine define)
	{
		m_Bits |= ComputeKeywordMask(define);
	}

	public void Disable(BuiltinShaderDefine define)
	{
		m_Bits &= ~ComputeKeywordMask(define);
	}
}
