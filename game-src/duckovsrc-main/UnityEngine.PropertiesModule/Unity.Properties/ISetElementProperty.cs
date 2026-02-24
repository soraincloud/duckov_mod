namespace Unity.Properties;

public interface ISetElementProperty : ICollectionElementProperty
{
	object ObjectKey { get; }
}
public interface ISetElementProperty<out TKey> : ISetElementProperty, ICollectionElementProperty
{
	TKey Key { get; }
}
