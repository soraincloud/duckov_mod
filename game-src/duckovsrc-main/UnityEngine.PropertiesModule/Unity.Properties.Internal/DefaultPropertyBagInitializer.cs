namespace Unity.Properties.Internal;

internal static class DefaultPropertyBagInitializer
{
	internal static void Initialize()
	{
		PropertyBag.Register(new ColorPropertyBag());
		PropertyBag.Register(new Vector2PropertyBag());
		PropertyBag.Register(new Vector3PropertyBag());
		PropertyBag.Register(new Vector4PropertyBag());
		PropertyBag.Register(new Vector2IntPropertyBag());
		PropertyBag.Register(new Vector3IntPropertyBag());
		PropertyBag.Register(new RectPropertyBag());
		PropertyBag.Register(new RectIntPropertyBag());
		PropertyBag.Register(new BoundsPropertyBag());
		PropertyBag.Register(new BoundsIntPropertyBag());
		PropertyBag.Register(new SystemVersionPropertyBag());
	}
}
