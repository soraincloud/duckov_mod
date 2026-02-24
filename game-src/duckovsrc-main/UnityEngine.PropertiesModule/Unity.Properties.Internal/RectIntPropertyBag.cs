using UnityEngine;

namespace Unity.Properties.Internal;

internal class RectIntPropertyBag : ContainerPropertyBag<RectInt>
{
	private class XProperty : Property<RectInt, int>
	{
		public override string Name => "x";

		public override bool IsReadOnly => false;

		public override int GetValue(ref RectInt container)
		{
			return container.x;
		}

		public override void SetValue(ref RectInt container, int value)
		{
			container.x = value;
		}
	}

	private class YProperty : Property<RectInt, int>
	{
		public override string Name => "y";

		public override bool IsReadOnly => false;

		public override int GetValue(ref RectInt container)
		{
			return container.y;
		}

		public override void SetValue(ref RectInt container, int value)
		{
			container.y = value;
		}
	}

	private class WidthProperty : Property<RectInt, int>
	{
		public override string Name => "width";

		public override bool IsReadOnly => false;

		public override int GetValue(ref RectInt container)
		{
			return container.width;
		}

		public override void SetValue(ref RectInt container, int value)
		{
			container.width = value;
		}
	}

	private class HeightProperty : Property<RectInt, int>
	{
		public override string Name => "height";

		public override bool IsReadOnly => false;

		public override int GetValue(ref RectInt container)
		{
			return container.height;
		}

		public override void SetValue(ref RectInt container, int value)
		{
			container.height = value;
		}
	}

	public RectIntPropertyBag()
	{
		AddProperty(new XProperty());
		AddProperty(new YProperty());
		AddProperty(new WidthProperty());
		AddProperty(new HeightProperty());
	}
}
