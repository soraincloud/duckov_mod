using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting;

public interface IState : IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable, IGraphElementWithData
{
	new StateGraph graph { get; }

	bool isStart { get; set; }

	bool canBeSource { get; }

	bool canBeDestination { get; }

	IEnumerable<IStateTransition> outgoingTransitions { get; }

	IEnumerable<IStateTransition> incomingTransitions { get; }

	IEnumerable<IStateTransition> transitions { get; }

	Vector2 position { get; set; }

	float width { get; set; }

	void OnBranchTo(Flow flow, IState destination);

	void OnEnter(Flow flow, StateEnterReason reason);

	void OnExit(Flow flow, StateExitReason reason);
}
