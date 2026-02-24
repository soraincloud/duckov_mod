using System;
using System.Diagnostics;

namespace Unity.VisualScripting;

[Conditional("UNITY_EDITOR")]
internal class VisualScriptingHelpURLAttribute : Attribute
{
	public VisualScriptingHelpURLAttribute(Type type)
	{
	}
}
