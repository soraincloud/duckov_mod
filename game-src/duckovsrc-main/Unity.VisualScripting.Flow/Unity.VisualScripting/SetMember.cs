namespace Unity.VisualScripting;

public sealed class SetMember : MemberUnit
{
	[Serialize]
	[InspectableIf("supportsChaining")]
	public bool chainable { get; set; }

	[DoNotSerialize]
	public bool supportsChaining => base.member.requiresTarget;

	[DoNotSerialize]
	[MemberFilter(Fields = true, Properties = true, ReadOnly = false)]
	public Member setter
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
	public ControlInput assign { get; private set; }

	[DoNotSerialize]
	[PortLabel("Value")]
	[PortLabelHidden]
	public ValueInput input { get; private set; }

	[DoNotSerialize]
	[PortLabel("Value")]
	[PortLabelHidden]
	public ValueOutput output { get; private set; }

	[DoNotSerialize]
	[PortLabel("Target")]
	[PortLabelHidden]
	public ValueOutput targetOutput { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ControlOutput assigned { get; private set; }

	public SetMember()
	{
	}

	public SetMember(Member member)
		: base(member)
	{
	}

	protected override void Definition()
	{
		base.Definition();
		assign = ControlInput("assign", Assign);
		assigned = ControlOutput("assigned");
		Succession(assign, assigned);
		if (supportsChaining && chainable)
		{
			targetOutput = ValueOutput(base.member.targetType, "targetOutput");
			Assignment(assign, targetOutput);
		}
		output = ValueOutput(base.member.type, "output");
		Assignment(assign, output);
		if (base.member.requiresTarget)
		{
			Requirement(base.target, assign);
		}
		input = ValueInput(base.member.type, "input");
		Requirement(input, assign);
		if (base.member.allowsNull)
		{
			input.AllowsNull();
		}
		input.SetDefaultValue(base.member.type.PseudoDefault());
	}

	protected override bool IsMemberValid(Member member)
	{
		if (member.isAccessor)
		{
			return member.isSettable;
		}
		return false;
	}

	private object GetAndChainTarget(Flow flow)
	{
		if (base.member.requiresTarget)
		{
			object value = flow.GetValue(base.target, base.member.targetType);
			if (supportsChaining && chainable)
			{
				flow.SetValue(targetOutput, value);
			}
			return value;
		}
		return null;
	}

	private ControlOutput Assign(Flow flow)
	{
		object andChainTarget = GetAndChainTarget(flow);
		object convertedValue = flow.GetConvertedValue(input);
		flow.SetValue(output, base.member.Set(andChainTarget, convertedValue));
		return assigned;
	}

	public override AnalyticsIdentifier GetAnalyticsIdentifier()
	{
		AnalyticsIdentifier obj = new AnalyticsIdentifier
		{
			Identifier = base.member.targetType.FullName + "." + base.member.name + "(Set)",
			Namespace = base.member.targetType.Namespace
		};
		obj.Hashcode = obj.Identifier.GetHashCode();
		return obj;
	}
}
