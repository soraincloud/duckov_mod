namespace Unity.VisualScripting;

[UnitCategory("Events/Lifecycle")]
[UnitOrder(3)]
[UnitTitle("On Update")]
public sealed class Update : MachineEventUnit<EmptyEventArgs>
{
	protected override string hookName => "Update";
}
