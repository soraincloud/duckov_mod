namespace Unity.VisualScripting;

[UnitCategory("Events/Lifecycle")]
[UnitOrder(4)]
[UnitTitle("On Fixed Update")]
public sealed class FixedUpdate : MachineEventUnit<EmptyEventArgs>
{
	protected override string hookName => "FixedUpdate";
}
