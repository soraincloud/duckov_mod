using System;
using System.Collections.Generic;

namespace Unity.VisualScripting;

[UnitCategory("Control")]
[UnitTitle("Select")]
[TypeIcon(typeof(ISelectUnit))]
[UnitOrder(6)]
public sealed class SelectUnit : Unit, ISelectUnit, IUnit, IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable
{
	[DoNotSerialize]
	[PortLabelHidden]
	public ValueInput condition { get; private set; }

	[DoNotSerialize]
	[PortLabel("True")]
	public ValueInput ifTrue { get; private set; }

	[DoNotSerialize]
	[PortLabel("False")]
	public ValueInput ifFalse { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueOutput selection { get; private set; }

	protected override void Definition()
	{
		condition = ValueInput<bool>("condition");
		ifTrue = ValueInput<object>("ifTrue").AllowsNull();
		ifFalse = ValueInput<object>("ifFalse").AllowsNull();
		selection = ValueOutput("selection", Branch).Predictable();
		Requirement(condition, selection);
		Requirement(ifTrue, selection);
		Requirement(ifFalse, selection);
	}

	public object Branch(Flow flow)
	{
		return flow.GetValue(flow.GetValue<bool>(condition) ? ifTrue : ifFalse);
	}

	FlowGraph IUnit.get_graph()
	{
		return base.graph;
	}
}
[TypeIcon(typeof(ISelectUnit))]
public abstract class SelectUnit<T> : Unit, ISelectUnit, IUnit, IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable
{
	[DoNotSerialize]
	public List<KeyValuePair<T, ValueInput>> branches { get; private set; }

	[Inspectable]
	[Serialize]
	public List<T> options { get; set; } = new List<T>();

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueInput selector { get; private set; }

	[DoNotSerialize]
	public ValueInput @default { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueOutput selection { get; private set; }

	public override bool canDefine => options != null;

	protected override void Definition()
	{
		selection = ValueOutput("selection", Result).Predictable();
		selector = ValueInput<T>("selector");
		Requirement(selector, selection);
		branches = new List<KeyValuePair<T, ValueInput>>();
		foreach (T option in options)
		{
			T val = option;
			string key = "%" + val;
			if (!base.valueInputs.Contains(key))
			{
				ValueInput valueInput = ValueInput<object>(key).AllowsNull();
				branches.Add(new KeyValuePair<T, ValueInput>(option, valueInput));
				Requirement(valueInput, selection);
			}
		}
		@default = ValueInput<object>("default");
		Requirement(@default, selection);
	}

	protected virtual bool Matches(T a, T b)
	{
		return object.Equals(a, b);
	}

	public object Result(Flow flow)
	{
		T value = flow.GetValue<T>(selector);
		foreach (KeyValuePair<T, ValueInput> branch in branches)
		{
			if (Matches(branch.Key, value))
			{
				return flow.GetValue(branch.Value);
			}
		}
		return flow.GetValue(@default);
	}

	FlowGraph IUnit.get_graph()
	{
		return base.graph;
	}
}
