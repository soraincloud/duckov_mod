using UnityEngine;

namespace Unity.Properties.Internal;

internal class Vector2IntPropertyBag : ContainerPropertyBag<Vector2Int>
{
	private class XProperty : Property<Vector2Int, int>
	{
		public override string Name => "x";

		public override bool IsReadOnly => false;

		public override int GetValue(ref Vector2Int container)
		{
			return container.x;
		}

		public override void SetValue(ref Vector2Int container, int value)
		{
			container.x = value;
		}
	}

	private class YProperty : Property<Vector2Int, int>
	{
		public override string Name => "y";

		public override bool IsReadOnly => false;

		public override int GetValue(ref Vector2Int container)
		{
			return container.y;
		}

		public override void SetValue(ref Vector2Int container, int value)
		{
			container.y = value;
		}
	}

	public Vector2IntPropertyBag()
	{
		AddProperty(new XProperty());
		AddProperty(new YProperty());
	}
}
