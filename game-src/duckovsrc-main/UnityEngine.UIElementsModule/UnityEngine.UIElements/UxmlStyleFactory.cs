namespace UnityEngine.UIElements;

public class UxmlStyleFactory : UxmlFactory<VisualElement, UxmlStyleTraits>
{
	internal const string k_ElementName = "Style";

	public override string uxmlName => "Style";

	public override string uxmlQualifiedName => uxmlNamespace + "." + uxmlName;

	public override string substituteForTypeName => typeof(VisualElement).Name;

	public override string substituteForTypeNamespace => typeof(VisualElement).Namespace ?? string.Empty;

	public override string substituteForTypeQualifiedName => typeof(VisualElement).FullName;

	public override VisualElement Create(IUxmlAttributes bag, CreationContext cc)
	{
		return null;
	}
}
