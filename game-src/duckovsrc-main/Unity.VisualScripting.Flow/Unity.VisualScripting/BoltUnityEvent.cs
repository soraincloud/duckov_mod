using System.ComponentModel;

namespace Unity.VisualScripting;

[UnitCategory("Events")]
[UnitTitle("UnityEvent")]
[UnitOrder(2)]
[DisplayName("Visual Scripting Unity Event")]
public sealed class BoltUnityEvent : MachineEventUnit<string>
{
	protected override string hookName => "UnityEvent";

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueInput name { get; private set; }

	protected override void Definition()
	{
		base.Definition();
		name = ValueInput("name", string.Empty);
	}

	protected override bool ShouldTrigger(Flow flow, string name)
	{
		return EventUnit<string>.CompareNames(flow, this.name, name);
	}
}
