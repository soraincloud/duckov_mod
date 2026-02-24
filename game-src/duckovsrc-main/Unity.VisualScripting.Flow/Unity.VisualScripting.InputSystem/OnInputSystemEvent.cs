using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.VisualScripting.InputSystem;

[UnitCategory("Events/Input")]
public abstract class OnInputSystemEvent : MachineEventUnit<EmptyEventArgs>
{
	private new class Data : EventUnit<EmptyEventArgs>.Data
	{
		internal InputAction Action;
	}

	[Serialize]
	[Inspectable]
	[UnitHeaderInspectable]
	public InputActionChangeOption InputActionChangeType;

	private Vector2 m_Value;

	protected override string hookName
	{
		get
		{
			if (UnityEngine.InputSystem.InputSystem.settings.updateMode != InputSettings.UpdateMode.ProcessEventsInDynamicUpdate)
			{
				return "FixedUpdate";
			}
			return "Update";
		}
	}

	protected abstract OutputType OutputType { get; }

	[DoNotSerialize]
	public ValueInput InputAction { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	[NullMeansSelf]
	public ValueInput Target { get; private set; }

	[PortLabelHidden]
	public ValueOutput FloatValue { get; private set; }

	[PortLabelHidden]
	public ValueOutput Vector2Value { get; private set; }

	public override IGraphElementData CreateData()
	{
		return new Data();
	}

	protected override void Definition()
	{
		base.Definition();
		Target = ValueInput(typeof(PlayerInput), "Target");
		Target.SetDefaultValue(null);
		Target.NullMeansSelf();
		InputAction = ValueInput(typeof(InputAction), "InputAction");
		InputAction.SetDefaultValue(null);
		switch (OutputType)
		{
		case OutputType.Float:
			FloatValue = ValueOutput("FloatValue", (Flow _) => m_Value.x);
			break;
		case OutputType.Vector2:
			Vector2Value = ValueOutput("Vector2Value", (Flow _) => m_Value);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case OutputType.Button:
			break;
		}
	}

	public override void StartListening(GraphStack stack)
	{
		base.StartListening(stack);
		GraphReference reference = stack.ToReference();
		PlayerInput playerInput = Flow.FetchValue<PlayerInput>(Target, reference);
		InputAction inputAction = Flow.FetchValue<InputAction>(InputAction, reference);
		if (inputAction != null)
		{
			stack.GetElementData<Data>(this).Action = (playerInput ? playerInput.actions.FindAction(inputAction.id) : ((inputAction.actionMap != null) ? inputAction : null));
		}
	}

	public override void StopListening(GraphStack stack)
	{
		base.StopListening(stack);
		stack.GetElementData<Data>(this).Action = null;
	}

	protected override bool ShouldTrigger(Flow flow, EmptyEventArgs args)
	{
		Data elementData = flow.stack.GetElementData<Data>(this);
		if (elementData.Action == null)
		{
			return false;
		}
		bool result = InputActionChangeType switch
		{
			InputActionChangeOption.OnPressed => elementData.Action.WasPressedThisFrame(), 
			InputActionChangeOption.OnHold => (OutputType == OutputType.Vector2) ? elementData.Action.IsInProgress() : elementData.Action.IsPressed(), 
			InputActionChangeOption.OnReleased => elementData.Action.WasReleasedThisFrame(), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		DoAssignArguments(flow, elementData);
		return result;
	}

	private void DoAssignArguments(Flow flow, Data data)
	{
		switch (OutputType)
		{
		case OutputType.Float:
		{
			float num = data.Action.ReadValue<float>();
			m_Value.Set(num, 0f);
			flow.SetValue(FloatValue, num);
			break;
		}
		case OutputType.Vector2:
			flow.SetValue(value: m_Value = data.Action.ReadValue<Vector2>(), port: Vector2Value);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case OutputType.Button:
			break;
		}
	}
}
