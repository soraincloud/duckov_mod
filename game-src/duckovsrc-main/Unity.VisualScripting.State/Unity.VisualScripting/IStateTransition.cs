using System;

namespace Unity.VisualScripting;

public interface IStateTransition : IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable, IConnection<IState, IState>
{
	void Branch(Flow flow);

	void OnEnter(Flow flow);

	void OnExit(Flow flow);
}
