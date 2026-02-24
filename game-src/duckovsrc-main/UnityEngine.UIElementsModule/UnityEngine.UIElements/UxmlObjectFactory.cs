namespace UnityEngine.UIElements;

internal class UxmlObjectFactory<TCreatedType, TTraits> : BaseUxmlFactory<TCreatedType, TTraits>, IUxmlObjectFactory<TCreatedType>, IBaseUxmlObjectFactory, IBaseUxmlFactory where TCreatedType : new() where TTraits : UxmlObjectTraits<TCreatedType>, new()
{
	public virtual TCreatedType CreateObject(IUxmlAttributes bag, CreationContext cc)
	{
		TCreatedType obj = new TCreatedType();
		m_Traits.Init(ref obj, bag, cc);
		return obj;
	}
}
