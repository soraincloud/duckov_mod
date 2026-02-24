using UnityEngine;

namespace Unity.Properties.Internal;

internal class BoundsIntPropertyBag : ContainerPropertyBag<BoundsInt>
{
	private class PositionProperty : Property<BoundsInt, Vector3Int>
	{
		public override string Name => "position";

		public override bool IsReadOnly => false;

		public override Vector3Int GetValue(ref BoundsInt container)
		{
			return container.position;
		}

		public override void SetValue(ref BoundsInt container, Vector3Int value)
		{
			container.position = value;
		}
	}

	private class SizeProperty : Property<BoundsInt, Vector3Int>
	{
		public override string Name => "size";

		public override bool IsReadOnly => false;

		public override Vector3Int GetValue(ref BoundsInt container)
		{
			return container.size;
		}

		public override void SetValue(ref BoundsInt container, Vector3Int value)
		{
			container.size = value;
		}
	}

	public BoundsIntPropertyBag()
	{
		AddProperty(new PositionProperty());
		AddProperty(new SizeProperty());
	}
}
