using UnityEngine;

namespace Unity.Properties.Internal;

internal class Vector3PropertyBag : ContainerPropertyBag<Vector3>
{
	private class XProperty : Property<Vector3, float>
	{
		public override string Name => "x";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Vector3 container)
		{
			return container.x;
		}

		public override void SetValue(ref Vector3 container, float value)
		{
			container.x = value;
		}
	}

	private class YProperty : Property<Vector3, float>
	{
		public override string Name => "y";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Vector3 container)
		{
			return container.y;
		}

		public override void SetValue(ref Vector3 container, float value)
		{
			container.y = value;
		}
	}

	private class ZProperty : Property<Vector3, float>
	{
		public override string Name => "z";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Vector3 container)
		{
			return container.z;
		}

		public override void SetValue(ref Vector3 container, float value)
		{
			container.z = value;
		}
	}

	public Vector3PropertyBag()
	{
		AddProperty(new XProperty());
		AddProperty(new YProperty());
		AddProperty(new ZProperty());
	}
}
