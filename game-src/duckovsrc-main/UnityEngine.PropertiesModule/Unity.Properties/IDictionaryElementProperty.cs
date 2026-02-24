namespace Unity.Properties;

public interface IDictionaryElementProperty : ICollectionElementProperty
{
	object ObjectKey { get; }
}
public interface IDictionaryElementProperty<out TKey> : IDictionaryElementProperty, ICollectionElementProperty
{
	TKey Key { get; }
}
