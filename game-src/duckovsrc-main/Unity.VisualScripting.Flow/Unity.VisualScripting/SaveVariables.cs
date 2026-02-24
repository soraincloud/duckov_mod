namespace Unity.VisualScripting;

[UnitCategory("Variables")]
public sealed class SaveVariables : Unit
{
	[DoNotSerialize]
	[PortLabelHidden]
	public ControlInput enter { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ControlOutput exit { get; private set; }

	protected override void Definition()
	{
		enter = ControlInput("enter", Enter);
		exit = ControlOutput("exit");
		Succession(enter, exit);
	}

	private ControlOutput Enter(Flow arg)
	{
		SavedVariables.SaveDeclarations(SavedVariables.merged);
		return exit;
	}
}
