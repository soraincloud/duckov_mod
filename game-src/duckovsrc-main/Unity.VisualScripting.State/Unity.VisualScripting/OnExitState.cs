namespace Unity.VisualScripting;

[UnitCategory("Events/State")]
public class OnExitState : ManualEventUnit<EmptyEventArgs>
{
	protected override string hookName => "OnExitState";
}
