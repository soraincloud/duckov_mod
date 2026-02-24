using System;
using System.Diagnostics;

namespace UnityEngine.TextCore.LowLevel;

[Serializable]
[DebuggerDisplay("Feature = {tag},  Lookup Count = {lookupIndexes.Length}")]
internal struct OpenTypeLayoutFeature
{
	public string tag;

	public uint[] lookupIndexes;
}
