namespace Unity.Properties;

public interface IVisitContravariantPropertyAdapter<TContainer, in TValue> : IPropertyVisitorAdapter
{
	void Visit(in VisitContext<TContainer> context, ref TContainer container, TValue value);
}
public interface IVisitContravariantPropertyAdapter<in TValue> : IPropertyVisitorAdapter
{
	void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, TValue value);
}
