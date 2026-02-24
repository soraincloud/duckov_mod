using UnityEngine;

namespace Unity.Properties.Internal;

internal class Vector4PropertyBag : ContainerPropertyBag<Vector4>
{
	private class XProperty : Property<Vector4, float>
	{
		public override string Name => "x";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Vector4 container)
		{
			return container.x;
		}

		public override void SetValue(ref Vector4 container, float value)
		{
			container.x = value;
		}
	}

	private class YProperty : Property<Vector4, float>
	{
		public override string Name => "y";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Vector4 container)
		{
			return container.y;
		}

		public override void SetValue(ref Vector4 container, float value)
		{
			container.y = value;
		}
	}

	private class ZProperty : Property<Vector4, float>
	{
		public override string Name => "z";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Vector4 container)
		{
			return container.z;
		}

		public override void SetValue(ref Vector4 container, float value)
		{
			container.z = value;
		}
	}

	private class WProperty : Property<Vector4, float>
	{
		public override string Name => "w";

		public override bool IsReadOnly => false;

		public override float GetValue(ref Vector4 container)
		{
			return container.w;
		}

		public override void SetValue(ref Vector4 container, float value)
		{
			container.w = value;
		}
	}

	public Vector4PropertyBag()
	{
		AddProperty(new XProperty());
		AddProperty(new YProperty());
		AddProperty(new ZProperty());
		AddProperty(new WProperty());
	}
}
