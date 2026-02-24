using System;
using UnityEngine;

namespace Unity.VisualScripting;

[SpecialUnit]
public abstract class UnifiedVariableUnit : Unit, IUnifiedVariableUnit, IUnit, IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable
{
	[Serialize]
	[Inspectable]
	[UnitHeaderInspectable]
	public VariableKind kind { get; set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueInput name { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	[NullMeansSelf]
	public ValueInput @object { get; private set; }

	protected override void Definition()
	{
		name = ValueInput("name", string.Empty);
		if (kind == VariableKind.Object)
		{
			@object = ValueInput<GameObject>("object", null).NullMeansSelf();
		}
	}

	FlowGraph IUnit.get_graph()
	{
		return base.graph;
	}
}
