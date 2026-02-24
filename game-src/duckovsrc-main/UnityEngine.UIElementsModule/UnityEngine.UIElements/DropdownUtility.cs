using System;

namespace UnityEngine.UIElements;

internal static class DropdownUtility
{
	internal static Func<IGenericMenu> MakeDropdownFunc;

	internal static IGenericMenu CreateDropdown()
	{
		IGenericMenu result;
		if (MakeDropdownFunc == null)
		{
			IGenericMenu genericMenu = new GenericDropdownMenu();
			result = genericMenu;
		}
		else
		{
			result = MakeDropdownFunc();
		}
		return result;
	}
}
