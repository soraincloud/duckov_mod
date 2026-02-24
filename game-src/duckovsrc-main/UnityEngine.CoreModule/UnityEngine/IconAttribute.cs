using System;
using System.Diagnostics;

namespace UnityEngine;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class IconAttribute : Attribute
{
	private string m_IconPath;

	public string path => m_IconPath;

	private IconAttribute()
	{
	}

	public IconAttribute(string path)
	{
		m_IconPath = path;
	}
}
