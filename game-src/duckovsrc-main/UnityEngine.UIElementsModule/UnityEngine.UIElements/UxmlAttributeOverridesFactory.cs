namespace UnityEngine.UIElements;

public class UxmlAttributeOverridesFactory : UxmlFactory<VisualElement, UxmlAttributeOverridesTraits>
{
	internal const string k_ElementName = "AttributeOverrides";

	public override string uxmlName => "AttributeOverrides";

	public override string uxmlQualifiedName => uxmlNamespace + "." + uxmlName;

	public override string substituteForTypeName => typeof(VisualElement).Name;

	public override string substituteForTypeNamespace => typeof(VisualElement).Namespace ?? string.Empty;

	public override string substituteForTypeQualifiedName => typeof(VisualElement).FullName;

	public override VisualElement Create(IUxmlAttributes bag, CreationContext cc)
	{
		return null;
	}
}
