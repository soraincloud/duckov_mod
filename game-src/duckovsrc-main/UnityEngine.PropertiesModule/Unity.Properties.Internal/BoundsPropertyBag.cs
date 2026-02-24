using UnityEngine;

namespace Unity.Properties.Internal;

internal class BoundsPropertyBag : ContainerPropertyBag<Bounds>
{
	private class CenterProperty : Property<Bounds, Vector3>
	{
		public override string Name => "center";

		public override bool IsReadOnly => false;

		public override Vector3 GetValue(ref Bounds container)
		{
			return container.center;
		}

		public override void SetValue(ref Bounds container, Vector3 value)
		{
			container.center = value;
		}
	}

	private class ExtentsProperty : Property<Bounds, Vector3>
	{
		public override string Name => "extents";

		public override bool IsReadOnly => false;

		public override Vector3 GetValue(ref Bounds container)
		{
			return container.extents;
		}

		public override void SetValue(ref Bounds container, Vector3 value)
		{
			container.extents = value;
		}
	}

	public BoundsPropertyBag()
	{
		AddProperty(new CenterProperty());
		AddProperty(new ExtentsProperty());
	}
}
