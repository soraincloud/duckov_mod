namespace Unity.VisualScripting;

public sealed class AnyState : State
{
	[DoNotSerialize]
	public override bool canBeDestination => false;

	public AnyState()
	{
		base.isStart = true;
	}

	public override void OnExit(Flow flow, StateExitReason reason)
	{
		if (reason != StateExitReason.Branch)
		{
			base.OnExit(flow, reason);
		}
	}

	public override void OnBranchTo(Flow flow, IState destination)
	{
		foreach (IStateTransition item in base.outgoingTransitionsNoAlloc)
		{
			if (item.destination != destination)
			{
				item.destination.OnExit(flow, StateExitReason.AnyBranch);
			}
		}
	}
}
