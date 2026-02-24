using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting;

[UnitSurtitle("Custom Event")]
[UnitShortTitle("Trigger")]
[TypeIcon(typeof(CustomEvent))]
[UnitCategory("Events")]
[UnitOrder(1)]
public sealed class TriggerCustomEvent : Unit
{
	[SerializeAs("argumentCount")]
	private int _argumentCount;

	[DoNotSerialize]
	public List<ValueInput> arguments { get; private set; }

	[DoNotSerialize]
	[Inspectable]
	[UnitHeaderInspectable("Arguments")]
	public int argumentCount
	{
		get
		{
			return _argumentCount;
		}
		set
		{
			_argumentCount = Mathf.Clamp(value, 0, 10);
		}
	}

	[DoNotSerialize]
	[PortLabelHidden]
	public ControlInput enter { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueInput name { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	[NullMeansSelf]
	public ValueInput target { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ControlOutput exit { get; private set; }

	protected override void Definition()
	{
		enter = ControlInput("enter", Trigger);
		exit = ControlOutput("exit");
		name = ValueInput("name", string.Empty);
		target = ValueInput<GameObject>("target", null).NullMeansSelf();
		arguments = new List<ValueInput>();
		for (int i = 0; i < argumentCount; i++)
		{
			ValueInput valueInput = ValueInput<object>("argument_" + i);
			arguments.Add(valueInput);
			Requirement(valueInput, enter);
		}
		Requirement(name, enter);
		Requirement(target, enter);
		Succession(enter, exit);
	}

	private ControlOutput Trigger(Flow flow)
	{
		GameObject value = flow.GetValue<GameObject>(target);
		string value2 = flow.GetValue<string>(name);
		object[] args = arguments.Select(flow.GetConvertedValue).ToArray();
		CustomEvent.Trigger(value, value2, args);
		return exit;
	}
}
