using UnityEngine;

namespace Unity.Properties.Internal;

internal class ColorPropertyBag : ContainerPropertyBag<Color>
{
	private class RProperty : Property<Color, float>
	{
		public override string Name => "r";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Color container)
		{
			return container.r;
		}

		public override void SetValue(ref Color container, float value)
		{
			container.r = value;
		}
	}

	private class GProperty : Property<Color, float>
	{
		public override string Name => "g";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Color container)
		{
			return container.g;
		}

		public override void SetValue(ref Color container, float value)
		{
			container.g = value;
		}
	}

	private class BProperty : Property<Color, float>
	{
		public override string Name => "b";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Color container)
		{
			return container.b;
		}

		public override void SetValue(ref Color container, float value)
		{
			container.b = value;
		}
	}

	private class AProperty : Property<Color, float>
	{
		public override string Name => "a";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Color container)
		{
			return container.a;
		}

		public override void SetValue(ref Color container, float value)
		{
			container.a = value;
		}
	}

	public ColorPropertyBag()
	{
		AddProperty(new RProperty());
		AddProperty(new GProperty());
		AddProperty(new BProperty());
		AddProperty(new AProperty());
	}
}
