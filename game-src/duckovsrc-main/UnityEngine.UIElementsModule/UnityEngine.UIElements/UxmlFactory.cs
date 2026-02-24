namespace UnityEngine.UIElements;

public class UxmlFactory<TCreatedType, TTraits> : BaseUxmlFactory<TCreatedType, TTraits>, IUxmlFactory, IBaseUxmlFactory where TCreatedType : VisualElement, new() where TTraits : UxmlTraits, new()
{
	public virtual VisualElement Create(IUxmlAttributes bag, CreationContext cc)
	{
		TCreatedType val = new TCreatedType();
		m_Traits.Init(val, bag, cc);
		return val;
	}
}
public class UxmlFactory<TCreatedType> : UxmlFactory<TCreatedType, VisualElement.UxmlTraits> where TCreatedType : VisualElement, new()
{
}
