using System.Collections.Generic;

namespace UnityEngine.UIElements;

public class TemplateContainer : BindableElement
{
	public new class UxmlFactory : UxmlFactory<TemplateContainer, UxmlTraits>
	{
		internal const string k_ElementName = "Instance";

		public override string uxmlName => "Instance";

		public override string uxmlQualifiedName => uxmlNamespace + "." + uxmlName;
	}

	public new class UxmlTraits : BindableElement.UxmlTraits
	{
		internal const string k_TemplateAttributeName = "template";

		private UxmlStringAttributeDescription m_Template = new UxmlStringAttributeDescription
		{
			name = "template",
			use = UxmlAttributeDescription.Use.Required
		};

		public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
		{
			get
			{
				yield break;
			}
		}

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			TemplateContainer templateContainer = (TemplateContainer)ve;
			templateContainer.templateId = m_Template.GetValueFromBag(bag, cc);
			VisualTreeAsset visualTreeAsset = cc.visualTreeAsset?.ResolveTemplate(templateContainer.templateId);
			if (visualTreeAsset == null)
			{
				templateContainer.Add(new Label($"Unknown Template: '{templateContainer.templateId}'"));
			}
			else
			{
				List<TemplateAsset.AttributeOverride> list = (bag as TemplateAsset)?.attributeOverrides;
				List<TemplateAsset.AttributeOverride> attributeOverrides = cc.attributeOverrides;
				List<TemplateAsset.AttributeOverride> list2 = null;
				if (list != null || attributeOverrides != null)
				{
					list2 = new List<TemplateAsset.AttributeOverride>();
					if (attributeOverrides != null)
					{
						list2.AddRange(attributeOverrides);
					}
					if (list != null)
					{
						list2.AddRange(list);
					}
				}
				visualTreeAsset.CloneTree(ve, cc.slotInsertionPoints, list2);
			}
			if (visualTreeAsset == null)
			{
				Debug.LogErrorFormat("Could not resolve template with name '{0}'", templateContainer.templateId);
			}
		}
	}

	private VisualElement m_ContentContainer;

	private VisualTreeAsset m_TemplateSource;

	public string templateId { get; private set; }

	public VisualTreeAsset templateSource
	{
		get
		{
			return m_TemplateSource;
		}
		internal set
		{
			m_TemplateSource = value;
		}
	}

	public override VisualElement contentContainer => m_ContentContainer;

	public TemplateContainer()
		: this(null)
	{
	}

	public TemplateContainer(string templateId)
	{
		this.templateId = templateId;
		m_ContentContainer = this;
	}

	internal void SetContentContainer(VisualElement content)
	{
		m_ContentContainer = content;
	}
}
