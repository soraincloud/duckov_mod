using System;
using System.ComponentModel;
using UnityEngine.Internal;

namespace UnityEngine;

[Obsolete("GUIElement has been removed. Consider using https://docs.unity3d.com/ScriptReference/UIElements.Image.html, https://docs.unity3d.com/ScriptReference/UIElements.TextElement.html or TextMeshPro instead.", true)]
[EditorBrowsable(EditorBrowsableState.Never)]
[ExcludeFromObjectFactory]
[ExcludeFromPreset]
public sealed class GUIElement
{
	private static void FeatureRemoved()
	{
		throw new Exception("GUIElement has been removed from Unity. Consider using https://docs.unity3d.com/ScriptReference/UIElements.Image.html, https://docs.unity3d.com/ScriptReference/UIElements.TextElement.html or TextMeshPro instead.");
	}

	[Obsolete("GUIElement has been removed. Consider using https://docs.unity3d.com/ScriptReference/UIElements.Image.html, https://docs.unity3d.com/ScriptReference/UIElements.TextElement.html or TextMeshPro instead.", true)]
	public bool HitTest(Vector3 screenPosition)
	{
		FeatureRemoved();
		return false;
	}

	[Obsolete("GUIElement has been removed. Consider using https://docs.unity3d.com/ScriptReference/UIElements.Image.html, https://docs.unity3d.com/ScriptReference/UIElements.TextElement.html or TextMeshPro instead.", true)]
	public bool HitTest(Vector3 screenPosition, [UnityEngine.Internal.DefaultValue("null")] Camera camera)
	{
		FeatureRemoved();
		return false;
	}

	[Obsolete("GUIElement has been removed. Consider using https://docs.unity3d.com/ScriptReference/UIElements.Image.html, https://docs.unity3d.com/ScriptReference/UIElements.TextElement.html or TextMeshPro instead.", true)]
	public Rect GetScreenRect([UnityEngine.Internal.DefaultValue("null")] Camera camera)
	{
		FeatureRemoved();
		return new Rect(0f, 0f, 0f, 0f);
	}

	[Obsolete("GUIElement has been removed. Consider using https://docs.unity3d.com/ScriptReference/UIElements.Image.html, https://docs.unity3d.com/ScriptReference/UIElements.TextElement.html or TextMeshPro instead.", true)]
	public Rect GetScreenRect()
	{
		FeatureRemoved();
		return new Rect(0f, 0f, 0f, 0f);
	}
}
