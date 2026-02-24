namespace Unity.Properties;

public interface IPropertyBag
{
	void Accept(ITypeVisitor visitor);

	void Accept(IPropertyBagVisitor visitor, ref object container);
}
public interface IPropertyBag<TContainer> : IPropertyBag
{
	PropertyCollection<TContainer> GetProperties();

	PropertyCollection<TContainer> GetProperties(ref TContainer container);

	TContainer CreateInstance();

	bool TryCreateInstance(out TContainer instance);

	void Accept(IPropertyBagVisitor visitor, ref TContainer container);
}
