namespace Unity.Properties;

internal interface IConstructor
{
	InstantiationKind InstantiationKind { get; }
}
internal interface IConstructor<out T> : IConstructor
{
	T Instantiate();
}
