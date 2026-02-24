using System;
using System.Diagnostics;

namespace UnityEngine.TextCore.LowLevel;

[Serializable]
[DebuggerDisplay("Language = {tag},  Feature Count = {featureIndexes.Length}")]
internal struct OpenTypeLayoutLanguage
{
	public string tag;

	public uint[] featureIndexes;
}
