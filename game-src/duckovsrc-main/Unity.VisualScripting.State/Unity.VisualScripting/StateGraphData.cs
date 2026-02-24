namespace Unity.VisualScripting;

public sealed class StateGraphData : GraphData<StateGraph>, IGraphEventListenerData, IGraphData
{
	public bool isListening { get; set; }

	public StateGraphData(StateGraph definition)
		: base(definition)
	{
	}
}
