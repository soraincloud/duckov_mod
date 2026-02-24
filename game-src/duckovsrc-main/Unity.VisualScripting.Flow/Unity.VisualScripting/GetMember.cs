namespace Unity.VisualScripting;

public sealed class GetMember : MemberUnit
{
	[DoNotSerialize]
	[MemberFilter(Fields = true, Properties = true, WriteOnly = false)]
	public Member getter
	{
		get
		{
			return base.member;
		}
		set
		{
			base.member = value;
		}
	}

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueOutput value { get; private set; }

	public GetMember()
	{
	}

	public GetMember(Member member)
		: base(member)
	{
	}

	protected override void Definition()
	{
		base.Definition();
		value = ValueOutput(base.member.type, "value", Value);
		if (base.member.isPredictable)
		{
			value.Predictable();
		}
		if (base.member.requiresTarget)
		{
			Requirement(base.target, value);
		}
	}

	protected override bool IsMemberValid(Member member)
	{
		if (member.isAccessor)
		{
			return member.isGettable;
		}
		return false;
	}

	private object Value(Flow flow)
	{
		object obj = (base.member.requiresTarget ? flow.GetValue(base.target, base.member.targetType) : null);
		return base.member.Get(obj);
	}

	public override AnalyticsIdentifier GetAnalyticsIdentifier()
	{
		AnalyticsIdentifier obj = new AnalyticsIdentifier
		{
			Identifier = base.member.targetType.FullName + "." + base.member.name + "(Get)",
			Namespace = base.member.targetType.Namespace
		};
		obj.Hashcode = obj.Identifier.GetHashCode();
		return obj;
	}
}
