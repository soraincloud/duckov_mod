using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnityEngine.TextCore.LowLevel;

[Serializable]
[DebuggerDisplay("Script = {tag},  Language Count = {languages.Count}")]
internal struct OpenTypeLayoutScript
{
	public string tag;

	public List<OpenTypeLayoutLanguage> languages;
}
