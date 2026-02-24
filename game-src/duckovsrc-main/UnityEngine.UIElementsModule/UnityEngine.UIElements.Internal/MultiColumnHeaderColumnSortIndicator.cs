namespace UnityEngine.UIElements.Internal;

internal class MultiColumnHeaderColumnSortIndicator : VisualElement
{
	public static readonly string ussClassName = MultiColumnHeaderColumn.ussClassName + "__sort-indicator";

	public static readonly string arrowUssClassName = ussClassName + "__arrow";

	public static readonly string indexLabelUssClassName = ussClassName + "__index-label";

	private Label m_IndexLabel;

	public string sortOrderLabel
	{
		get
		{
			return m_IndexLabel.text;
		}
		set
		{
			m_IndexLabel.text = value;
		}
	}

	public MultiColumnHeaderColumnSortIndicator()
	{
		AddToClassList(ussClassName);
		base.pickingMode = PickingMode.Ignore;
		VisualElement visualElement = new VisualElement
		{
			pickingMode = PickingMode.Ignore
		};
		visualElement.AddToClassList(arrowUssClassName);
		Add(visualElement);
		m_IndexLabel = new Label
		{
			pickingMode = PickingMode.Ignore
		};
		m_IndexLabel.AddToClassList(indexLabelUssClassName);
		Add(m_IndexLabel);
	}
}
