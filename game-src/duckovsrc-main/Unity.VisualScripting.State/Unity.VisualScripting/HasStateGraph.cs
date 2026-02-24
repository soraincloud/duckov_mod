using JetBrains.Annotations;

namespace Unity.VisualScripting;

[TypeIcon(typeof(StateGraph))]
[UnitCategory("Graphs/Graph Nodes")]
public sealed class HasStateGraph : HasGraph<StateGraph, StateGraphAsset, StateMachine>
{
	[Serialize]
	[Inspectable]
	[UnitHeaderInspectable]
	[UsedImplicitly]
	public StateGraphContainerType containerType { get; set; }

	protected override bool isGameObject => containerType == StateGraphContainerType.GameObject;
}
