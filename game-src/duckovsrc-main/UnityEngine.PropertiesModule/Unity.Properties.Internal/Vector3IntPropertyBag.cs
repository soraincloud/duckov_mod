using UnityEngine;

namespace Unity.Properties.Internal;

internal class Vector3IntPropertyBag : ContainerPropertyBag<Vector3Int>
{
	private class XProperty : Property<Vector3Int, int>
	{
		public override string Name => "x";

		public override bool IsReadOnly => false;

		public override int GetValue(ref Vector3Int container)
		{
			return container.x;
		}

		public override void SetValue(ref Vector3Int container, int value)
		{
			container.x = value;
		}
	}

	private class YProperty : Property<Vector3Int, int>
	{
		public override string Name => "y";

		public override bool IsReadOnly => false;

		public override int GetValue(ref Vector3Int container)
		{
			return container.y;
		}

		public override void SetValue(ref Vector3Int container, int value)
		{
			container.y = value;
		}
	}

	private class ZProperty : Property<Vector3Int, int>
	{
		public override string Name => "z";

		public override bool IsReadOnly => false;

		public override int GetValue(ref Vector3Int container)
		{
			return container.z;
		}

		public override void SetValue(ref Vector3Int container, int value)
		{
			container.z = value;
		}
	}

	public Vector3IntPropertyBag()
	{
		AddProperty(new XProperty());
		AddProperty(new YProperty());
		AddProperty(new ZProperty());
	}
}
