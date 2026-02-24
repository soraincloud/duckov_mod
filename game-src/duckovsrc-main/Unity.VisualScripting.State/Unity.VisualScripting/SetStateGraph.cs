using JetBrains.Annotations;

namespace Unity.VisualScripting;

[TypeIcon(typeof(StateGraph))]
public class SetStateGraph : SetGraph<StateGraph, StateGraphAsset, StateMachine>
{
	[Serialize]
	[Inspectable]
	[UnitHeaderInspectable]
	[UsedImplicitly]
	public StateGraphContainerType containerType { get; set; }

	protected override bool isGameObject => containerType == StateGraphContainerType.GameObject;
}
