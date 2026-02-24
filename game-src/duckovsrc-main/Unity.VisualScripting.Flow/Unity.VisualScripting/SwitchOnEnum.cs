using System;
using System.Collections.Generic;

namespace Unity.VisualScripting;

[UnitCategory("Control")]
[UnitTitle("Switch On Enum")]
[UnitShortTitle("Switch")]
[UnitSubtitle("On Enum")]
[UnitOrder(3)]
[TypeIcon(typeof(IBranchUnit))]
public sealed class SwitchOnEnum : Unit, IBranchUnit, IUnit, IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable
{
	[DoNotSerialize]
	public Dictionary<Enum, ControlOutput> branches { get; private set; }

	[Serialize]
	[Inspectable]
	[UnitHeaderInspectable]
	[TypeFilter(new Type[] { }, Enums = true, Classes = false, Interfaces = false, Structs = false, Primitives = false)]
	public Type enumType { get; set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ControlInput enter { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueInput @enum { get; private set; }

	public override bool canDefine
	{
		get
		{
			if (enumType != null)
			{
				return enumType.IsEnum;
			}
			return false;
		}
	}

	protected override void Definition()
	{
		branches = new Dictionary<Enum, ControlOutput>();
		enter = ControlInput("enter", Enter);
		@enum = ValueInput(enumType, "enum");
		Requirement(@enum, enter);
		foreach (KeyValuePair<string, Enum> item in EnumUtility.ValuesByNames(enumType))
		{
			string key = item.Key;
			Enum value = item.Value;
			if (!branches.ContainsKey(value))
			{
				ControlOutput controlOutput = ControlOutput("%" + key);
				branches.Add(value, controlOutput);
				Succession(enter, controlOutput);
			}
		}
	}

	public ControlOutput Enter(Flow flow)
	{
		Enum key = (Enum)flow.GetValue(@enum, enumType);
		if (branches.ContainsKey(key))
		{
			return branches[key];
		}
		return null;
	}

	FlowGraph IUnit.get_graph()
	{
		return base.graph;
	}
}
