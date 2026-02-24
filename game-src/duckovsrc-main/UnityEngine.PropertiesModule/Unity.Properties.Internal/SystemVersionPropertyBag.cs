using System;
using UnityEngine;

namespace Unity.Properties.Internal;

internal class SystemVersionPropertyBag : ContainerPropertyBag<Version>
{
	private class MajorProperty : Property<Version, int>
	{
		public override string Name => "Major";

		public override bool IsReadOnly => true;

		public MajorProperty()
		{
			AddAttribute(new MinAttribute(0f));
		}

		public override int GetValue(ref Version container)
		{
			return container.Major;
		}

		public override void SetValue(ref Version container, int value)
		{
		}
	}

	private class MinorProperty : Property<Version, int>
	{
		public override string Name => "Minor";

		public override bool IsReadOnly => true;

		public MinorProperty()
		{
			AddAttribute(new MinAttribute(0f));
		}

		public override int GetValue(ref Version container)
		{
			return container.Minor;
		}

		public override void SetValue(ref Version container, int value)
		{
		}
	}

	private class BuildProperty : Property<Version, int>
	{
		public override string Name => "Build";

		public override bool IsReadOnly => true;

		public BuildProperty()
		{
			AddAttribute(new MinAttribute(0f));
		}

		public override int GetValue(ref Version container)
		{
			return container.Build;
		}

		public override void SetValue(ref Version container, int value)
		{
		}
	}

	private class RevisionProperty : Property<Version, int>
	{
		public override string Name => "Revision";

		public override bool IsReadOnly => true;

		public RevisionProperty()
		{
			AddAttribute(new MinAttribute(0f));
		}

		public override int GetValue(ref Version container)
		{
			return container.Revision;
		}

		public override void SetValue(ref Version container, int value)
		{
		}
	}

	public SystemVersionPropertyBag()
	{
		AddProperty(new MajorProperty());
		AddProperty(new MinorProperty());
		AddProperty(new BuildProperty());
		AddProperty(new RevisionProperty());
	}
}
