using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public class DropdownMenu
{
	private List<DropdownMenuItem> m_MenuItems = new List<DropdownMenuItem>();

	private DropdownMenuEventInfo m_DropdownMenuEventInfo;

	public List<DropdownMenuItem> MenuItems()
	{
		return m_MenuItems;
	}

	public void AppendAction(string actionName, Action<DropdownMenuAction> action, Func<DropdownMenuAction, DropdownMenuAction.Status> actionStatusCallback, object userData = null)
	{
		DropdownMenuAction item = new DropdownMenuAction(actionName, action, actionStatusCallback, userData);
		m_MenuItems.Add(item);
	}

	public void AppendAction(string actionName, Action<DropdownMenuAction> action, DropdownMenuAction.Status status = DropdownMenuAction.Status.Normal)
	{
		if (status == DropdownMenuAction.Status.Normal)
		{
			AppendAction(actionName, action, DropdownMenuAction.AlwaysEnabled);
			return;
		}
		if (status == DropdownMenuAction.Status.Disabled)
		{
			AppendAction(actionName, action, DropdownMenuAction.AlwaysDisabled);
			return;
		}
		AppendAction(actionName, action, (DropdownMenuAction e) => status);
	}

	public void InsertAction(int atIndex, string actionName, Action<DropdownMenuAction> action, Func<DropdownMenuAction, DropdownMenuAction.Status> actionStatusCallback, object userData = null)
	{
		DropdownMenuAction item = new DropdownMenuAction(actionName, action, actionStatusCallback, userData);
		m_MenuItems.Insert(atIndex, item);
	}

	public void InsertAction(int atIndex, string actionName, Action<DropdownMenuAction> action, DropdownMenuAction.Status status = DropdownMenuAction.Status.Normal)
	{
		if (status == DropdownMenuAction.Status.Normal)
		{
			InsertAction(atIndex, actionName, action, DropdownMenuAction.AlwaysEnabled);
			return;
		}
		if (status == DropdownMenuAction.Status.Disabled)
		{
			InsertAction(atIndex, actionName, action, DropdownMenuAction.AlwaysDisabled);
			return;
		}
		InsertAction(atIndex, actionName, action, (DropdownMenuAction e) => status);
	}

	public void AppendSeparator(string subMenuPath = null)
	{
		if (m_MenuItems.Count > 0 && !(m_MenuItems[m_MenuItems.Count - 1] is DropdownMenuSeparator))
		{
			DropdownMenuSeparator item = new DropdownMenuSeparator(subMenuPath ?? string.Empty);
			m_MenuItems.Add(item);
		}
	}

	public void InsertSeparator(string subMenuPath, int atIndex)
	{
		if (atIndex > 0 && atIndex <= m_MenuItems.Count && !(m_MenuItems[atIndex - 1] is DropdownMenuSeparator))
		{
			DropdownMenuSeparator item = new DropdownMenuSeparator(subMenuPath ?? string.Empty);
			m_MenuItems.Insert(atIndex, item);
		}
	}

	public void RemoveItemAt(int index)
	{
		m_MenuItems.RemoveAt(index);
	}

	public void ClearItems()
	{
		m_MenuItems.Clear();
	}

	public void PrepareForDisplay(EventBase e)
	{
		m_DropdownMenuEventInfo = ((e != null) ? new DropdownMenuEventInfo(e) : null);
		if (m_MenuItems.Count == 0)
		{
			return;
		}
		foreach (DropdownMenuItem menuItem in m_MenuItems)
		{
			if (menuItem is DropdownMenuAction dropdownMenuAction)
			{
				dropdownMenuAction.UpdateActionStatus(m_DropdownMenuEventInfo);
			}
		}
		if (m_MenuItems[m_MenuItems.Count - 1] is DropdownMenuSeparator)
		{
			m_MenuItems.RemoveAt(m_MenuItems.Count - 1);
		}
	}
}
