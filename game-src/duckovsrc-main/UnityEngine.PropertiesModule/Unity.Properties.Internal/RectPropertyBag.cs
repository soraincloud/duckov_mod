using UnityEngine;

namespace Unity.Properties.Internal;

internal class RectPropertyBag : ContainerPropertyBag<Rect>
{
	private class XProperty : Property<Rect, float>
	{
		public override string Name => "x";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Rect container)
		{
			return container.x;
		}

		public override void SetValue(ref Rect container, float value)
		{
			container.x = value;
		}
	}

	private class YProperty : Property<Rect, float>
	{
		public override string Name => "y";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Rect container)
		{
			return container.y;
		}

		public override void SetValue(ref Rect container, float value)
		{
			container.y = value;
		}
	}

	private class WidthProperty : Property<Rect, float>
	{
		public override string Name => "width";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Rect container)
		{
			return container.width;
		}

		public override void SetValue(ref Rect container, float value)
		{
			container.width = value;
		}
	}

	private class HeightProperty : Property<Rect, float>
	{
		public override string Name => "height";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Rect container)
		{
			return container.height;
		}

		public override void SetValue(ref Rect container, float value)
		{
			container.height = value;
		}
	}

	public RectPropertyBag()
	{
		AddProperty(new XProperty());
		AddProperty(new YProperty());
		AddProperty(new WidthProperty());
		AddProperty(new HeightProperty());
	}
}
