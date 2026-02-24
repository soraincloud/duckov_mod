using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UIElements;

internal class DefaultDragAndDropClient : DragAndDropData, IDragAndDrop
{
	private readonly Hashtable m_GenericData = new Hashtable();

	private Label m_DraggedInfoLabel;

	private DragVisualMode m_VisualMode;

	private IEnumerable<Object> m_UnityObjectReferences;

	public override DragVisualMode visualMode => m_VisualMode;

	public override object source => GetGenericData("__unity-drag-and-drop__source-view");

	public override IEnumerable<Object> unityObjectReferences => m_UnityObjectReferences;

	public DragAndDropData data => this;

	public override object GetGenericData(string key)
	{
		return m_GenericData.ContainsKey(key) ? m_GenericData[key] : null;
	}

	public override void SetGenericData(string key, object value)
	{
		m_GenericData[key] = value;
	}

	public void StartDrag(StartDragArgs args, Vector3 pointerPosition)
	{
		if (args.unityObjectReferences != null)
		{
			m_UnityObjectReferences = args.unityObjectReferences.ToArray();
		}
		m_VisualMode = args.visualMode;
		foreach (DictionaryEntry genericDatum in args.genericData)
		{
			m_GenericData[(string)genericDatum.Key] = genericDatum.Value;
		}
		if (string.IsNullOrWhiteSpace(args.title))
		{
			return;
		}
		VisualElement visualElement = ((source is VisualElement visualElement2) ? visualElement2.panel.visualTree : null);
		if (visualElement != null)
		{
			if (m_DraggedInfoLabel == null)
			{
				Label label = new Label();
				label.pickingMode = PickingMode.Ignore;
				label.style.position = Position.Absolute;
				m_DraggedInfoLabel = label;
			}
			m_DraggedInfoLabel.text = args.title;
			m_DraggedInfoLabel.style.top = pointerPosition.y;
			m_DraggedInfoLabel.style.left = pointerPosition.x;
			visualElement.Add(m_DraggedInfoLabel);
		}
	}

	public void UpdateDrag(Vector3 pointerPosition)
	{
		if (m_DraggedInfoLabel != null)
		{
			m_DraggedInfoLabel.style.top = pointerPosition.y;
			m_DraggedInfoLabel.style.left = pointerPosition.x;
		}
	}

	public void AcceptDrag()
	{
	}

	public void SetVisualMode(DragVisualMode mode)
	{
		m_VisualMode = mode;
	}

	public void DragCleanup()
	{
		m_UnityObjectReferences = null;
		m_GenericData?.Clear();
		SetVisualMode(DragVisualMode.None);
		m_DraggedInfoLabel?.RemoveFromHierarchy();
	}
}
