using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class DefaultGroupManager : IGroupManager
{
	private List<IGroupBoxOption> m_GroupOptions = new List<IGroupBoxOption>();

	private IGroupBoxOption m_SelectedOption;

	private IGroupBox m_GroupBox;

	public void Init(IGroupBox groupBox)
	{
		m_GroupBox = groupBox;
	}

	public IGroupBoxOption GetSelectedOption()
	{
		return m_SelectedOption;
	}

	public void OnOptionSelectionChanged(IGroupBoxOption selectedOption)
	{
		if (m_SelectedOption == selectedOption)
		{
			return;
		}
		m_SelectedOption = selectedOption;
		foreach (IGroupBoxOption groupOption in m_GroupOptions)
		{
			groupOption.SetSelected(groupOption == m_SelectedOption);
		}
	}

	public void RegisterOption(IGroupBoxOption option)
	{
		if (!m_GroupOptions.Contains(option))
		{
			m_GroupOptions.Add(option);
			m_GroupBox.OnOptionAdded(option);
		}
	}

	public void UnregisterOption(IGroupBoxOption option)
	{
		m_GroupOptions.Remove(option);
		m_GroupBox.OnOptionRemoved(option);
	}
}
