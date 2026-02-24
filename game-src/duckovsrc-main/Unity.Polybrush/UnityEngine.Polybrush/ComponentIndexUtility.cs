using System;

namespace UnityEngine.Polybrush;

internal static class ComponentIndexUtility
{
	internal static readonly GUIContent[] ComponentIndexPopupDescriptions = new GUIContent[4]
	{
		new GUIContent("R"),
		new GUIContent("G"),
		new GUIContent("B"),
		new GUIContent("A")
	};

	internal static readonly int[] ComponentIndexPopupValues = new int[4] { 0, 1, 2, 3 };

	internal static uint ToFlag(this ComponentIndex e)
	{
		if (!Enum.IsDefined(typeof(ComponentIndex), e))
		{
			return (uint)e;
		}
		int num = (int)(e + 1);
		if (num >= 3)
		{
			if (num != 3)
			{
				return 8u;
			}
			return 4u;
		}
		return (uint)num;
	}

	internal static string GetString(this ComponentIndex component, ComponentIndexType type = ComponentIndexType.Vector)
	{
		if (!Enum.IsDefined(typeof(ComponentIndex), component))
		{
			int num = (int)component;
			return num.ToString();
		}
		int num2 = (int)component;
		return type switch
		{
			ComponentIndexType.Vector => num2 switch
			{
				2 => "Z", 
				1 => "Y", 
				0 => "X", 
				_ => "W", 
			}, 
			ComponentIndexType.Color => num2 switch
			{
				2 => "B", 
				1 => "G", 
				0 => "R", 
				_ => "A", 
			}, 
			_ => num2.ToString(), 
		};
	}
}
