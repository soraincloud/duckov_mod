using UnityEngine;

namespace Unity.Properties.Internal;

internal class Vector2PropertyBag : ContainerPropertyBag<Vector2>
{
	private class XProperty : Property<Vector2, float>
	{
		public override string Name => "x";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Vector2 container)
		{
			return container.x;
		}

		public override void SetValue(ref Vector2 container, float value)
		{
			container.x = value;
		}
	}

	private class YProperty : Property<Vector2, float>
	{
		public override string Name => "y";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Vector2 container)
		{
			return container.y;
		}

		public override void SetValue(ref Vector2 container, float value)
		{
			container.y = value;
		}
	}

	public Vector2PropertyBag()
	{
		AddProperty(new XProperty());
		AddProperty(new YProperty());
	}
}
