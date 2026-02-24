using System;

namespace Unity.VisualScripting;

public abstract class StateTransition : GraphElement<StateGraph>, IStateTransition, IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable, IConnection<IState, IState>
{
	public class DebugData : IStateTransitionDebugData, IGraphElementDebugData
	{
		public Exception runtimeException { get; set; }

		public int lastBranchFrame { get; set; }

		public float lastBranchTime { get; set; }
	}

	public override int dependencyOrder => 1;

	[Serialize]
	public IState source { get; internal set; }

	[Serialize]
	public IState destination { get; internal set; }

	protected StateTransition()
	{
	}

	protected StateTransition(IState source, IState destination)
	{
		Ensure.That("source").IsNotNull(source);
		Ensure.That("destination").IsNotNull(destination);
		if (source.graph != destination.graph)
		{
			throw new NotSupportedException("Cannot create transitions across state graphs.");
		}
		this.source = source;
		this.destination = destination;
	}

	public IGraphElementDebugData CreateDebugData()
	{
		return new DebugData();
	}

	public override void Instantiate(GraphReference instance)
	{
		base.Instantiate(instance);
		if (this is IGraphEventListener listener && instance.GetElementData<State.Data>(source).isActive)
		{
			listener.StartListening(instance);
		}
	}

	public override void Uninstantiate(GraphReference instance)
	{
		if (this is IGraphEventListener listener)
		{
			listener.StopListening(instance);
		}
		base.Uninstantiate(instance);
	}

	public void Branch(Flow flow)
	{
		if (flow.enableDebug)
		{
			DebugData elementDebugData = flow.stack.GetElementDebugData<DebugData>(this);
			elementDebugData.lastBranchFrame = EditorTimeBinding.frame;
			elementDebugData.lastBranchTime = EditorTimeBinding.time;
		}
		try
		{
			source.OnExit(flow, StateExitReason.Branch);
		}
		catch (Exception ex)
		{
			source.HandleException(flow.stack, ex);
			throw;
		}
		source.OnBranchTo(flow, destination);
		try
		{
			destination.OnEnter(flow, StateEnterReason.Branch);
		}
		catch (Exception ex2)
		{
			destination.HandleException(flow.stack, ex2);
			throw;
		}
	}

	public abstract void OnEnter(Flow flow);

	public abstract void OnExit(Flow flow);

	public override AnalyticsIdentifier GetAnalyticsIdentifier()
	{
		return null;
	}
}
