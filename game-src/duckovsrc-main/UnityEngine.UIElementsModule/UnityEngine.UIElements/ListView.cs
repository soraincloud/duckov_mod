using System;
using System.Collections;

namespace UnityEngine.UIElements;

public class ListView : BaseListView
{
	public new class UxmlFactory : UxmlFactory<ListView, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseListView.UxmlTraits
	{
	}

	private Func<VisualElement> m_MakeItem;

	private Action<VisualElement, int> m_BindItem;

	public new Func<VisualElement> makeItem
	{
		get
		{
			return m_MakeItem;
		}
		set
		{
			if (value != m_MakeItem)
			{
				m_MakeItem = value;
				Rebuild();
			}
		}
	}

	public new Action<VisualElement, int> bindItem
	{
		get
		{
			return m_BindItem;
		}
		set
		{
			if (value != m_BindItem)
			{
				m_BindItem = value;
				RefreshItems();
			}
		}
	}

	public new Action<VisualElement, int> unbindItem { get; set; }

	public new Action<VisualElement> destroyItem { get; set; }

	internal void SetMakeItemWithoutNotify(Func<VisualElement> func)
	{
		m_MakeItem = func;
	}

	internal void SetBindItemWithoutNotify(Action<VisualElement, int> callback)
	{
		m_BindItem = callback;
	}

	internal override bool HasValidDataAndBindings()
	{
		return base.HasValidDataAndBindings() && makeItem != null == (bindItem != null);
	}

	protected override CollectionViewController CreateViewController()
	{
		return new ListViewController();
	}

	public ListView()
	{
		AddToClassList(BaseListView.ussClassName);
	}

	public ListView(IList itemsSource, float itemHeight = -1f, Func<VisualElement> makeItem = null, Action<VisualElement, int> bindItem = null)
		: base(itemsSource, itemHeight)
	{
		AddToClassList(BaseListView.ussClassName);
		this.makeItem = makeItem;
		this.bindItem = bindItem;
	}
}
