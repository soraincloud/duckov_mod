namespace UnityEngine.UIElements;

internal class VisualElementPanelActivator
{
	private IVisualElementPanelActivatable m_Activatable;

	private EventCallback<AttachToPanelEvent> m_OnAttachToPanelCallback;

	private EventCallback<DetachFromPanelEvent> m_OnDetachFromPanelCallback;

	public bool isActive { get; private set; }

	public bool isDetaching { get; private set; }

	public VisualElementPanelActivator(IVisualElementPanelActivatable activatable)
	{
		m_Activatable = activatable;
		m_OnAttachToPanelCallback = OnEnter;
		m_OnDetachFromPanelCallback = OnLeave;
	}

	public void SetActive(bool action)
	{
		if (isActive != action)
		{
			isActive = action;
			if (isActive)
			{
				m_Activatable.element.RegisterCallback(m_OnAttachToPanelCallback);
				m_Activatable.element.RegisterCallback(m_OnDetachFromPanelCallback);
				SendActivation();
			}
			else
			{
				m_Activatable.element.UnregisterCallback(m_OnAttachToPanelCallback);
				m_Activatable.element.UnregisterCallback(m_OnDetachFromPanelCallback);
				SendDeactivation();
			}
		}
	}

	public void SendActivation()
	{
		if (m_Activatable.CanBeActivated())
		{
			m_Activatable.OnPanelActivate();
		}
	}

	public void SendDeactivation()
	{
		if (m_Activatable.CanBeActivated())
		{
			m_Activatable.OnPanelDeactivate();
		}
	}

	private void OnEnter(AttachToPanelEvent evt)
	{
		if (isActive)
		{
			SendActivation();
		}
	}

	private void OnLeave(DetachFromPanelEvent evt)
	{
		if (isActive)
		{
			isDetaching = true;
			try
			{
				SendDeactivation();
			}
			finally
			{
				isDetaching = false;
			}
		}
	}
}
