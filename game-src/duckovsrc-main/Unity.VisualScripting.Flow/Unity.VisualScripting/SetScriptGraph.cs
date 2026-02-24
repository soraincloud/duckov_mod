using JetBrains.Annotations;

namespace Unity.VisualScripting;

[TypeIcon(typeof(FlowGraph))]
public sealed class SetScriptGraph : SetGraph<FlowGraph, ScriptGraphAsset, ScriptMachine>
{
	[Serialize]
	[Inspectable]
	[UnitHeaderInspectable]
	[UsedImplicitly]
	public ScriptGraphContainerType containerType { get; set; }

	protected override bool isGameObject => containerType == ScriptGraphContainerType.GameObject;
}
