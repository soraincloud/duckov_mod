namespace UnityEngine.UIElements;

public class UxmlTemplateFactory : UxmlFactory<VisualElement, UxmlTemplateTraits>
{
	internal const string k_ElementName = "Template";

	public override string uxmlName => "Template";

	public override string uxmlQualifiedName => uxmlNamespace + "." + uxmlName;

	public override string substituteForTypeName => typeof(VisualElement).Name;

	public override string substituteForTypeNamespace => typeof(VisualElement).Namespace ?? string.Empty;

	public override string substituteForTypeQualifiedName => typeof(VisualElement).FullName;

	public override VisualElement Create(IUxmlAttributes bag, CreationContext cc)
	{
		return null;
	}
}
