namespace Unity.Properties;

public interface IExcludePropertyAdapter<TContainer, TValue> : IPropertyVisitorAdapter
{
	bool IsExcluded(in ExcludeContext<TContainer, TValue> context, ref TContainer container, ref TValue value);
}
public interface IExcludePropertyAdapter<TValue> : IPropertyVisitorAdapter
{
	bool IsExcluded<TContainer>(in ExcludeContext<TContainer, TValue> context, ref TContainer container, ref TValue value);
}
public interface IExcludePropertyAdapter : IPropertyVisitorAdapter
{
	bool IsExcluded<TContainer, TValue>(in ExcludeContext<TContainer, TValue> context, ref TContainer container, ref TValue value);
}
