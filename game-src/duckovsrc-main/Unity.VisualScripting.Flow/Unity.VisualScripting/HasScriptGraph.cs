using JetBrains.Annotations;

namespace Unity.VisualScripting;

[TypeIcon(typeof(FlowGraph))]
[UnitCategory("Graphs/Graph Nodes")]
public sealed class HasScriptGraph : HasGraph<FlowGraph, ScriptGraphAsset, ScriptMachine>
{
	[Serialize]
	[Inspectable]
	[UnitHeaderInspectable]
	[UsedImplicitly]
	public ScriptGraphContainerType containerType { get; set; }

	protected override bool isGameObject => containerType == ScriptGraphContainerType.GameObject;
}
