namespace Unity.Properties;

public interface IVisitPropertyAdapter<TContainer, TValue> : IPropertyVisitorAdapter
{
	void Visit(in VisitContext<TContainer, TValue> context, ref TContainer container, ref TValue value);
}
public interface IVisitPropertyAdapter<TValue> : IPropertyVisitorAdapter
{
	void Visit<TContainer>(in VisitContext<TContainer, TValue> context, ref TContainer container, ref TValue value);
}
public interface IVisitPropertyAdapter : IPropertyVisitorAdapter
{
	void Visit<TContainer, TValue>(in VisitContext<TContainer, TValue> context, ref TContainer container, ref TValue value);
}
