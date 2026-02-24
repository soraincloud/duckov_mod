using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting;

public abstract class NesterStateTransition<TGraph, TMacro> : StateTransition, INesterStateTransition, IStateTransition, IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable, IConnection<IState, IState>, IGraphNesterElement, IGraphParentElement, IGraphParent, IGraphNester where TGraph : class, IGraph, new() where TMacro : Macro<TGraph>
{
	[Serialize]
	public GraphNest<TGraph, TMacro> nest { get; private set; } = new GraphNest<TGraph, TMacro>();

	[DoNotSerialize]
	IGraphNest IGraphNester.nest => nest;

	[DoNotSerialize]
	IGraph IGraphParent.childGraph => nest.graph;

	[DoNotSerialize]
	bool IGraphParent.isSerializationRoot => nest.source == GraphSource.Macro;

	[DoNotSerialize]
	UnityEngine.Object IGraphParent.serializedObject => nest.macro;

	[DoNotSerialize]
	public override IEnumerable<ISerializationDependency> deserializationDependencies => nest.deserializationDependencies;

	protected NesterStateTransition()
	{
		nest.nester = this;
	}

	protected NesterStateTransition(IState source, IState destination)
		: base(source, destination)
	{
		nest.nester = this;
	}

	public override IEnumerable<object> GetAotStubs(HashSet<object> visited)
	{
		return LinqUtility.Concat<object>(new IEnumerable[2]
		{
			base.GetAotStubs(visited),
			nest.GetAotStubs(visited)
		});
	}

	protected void CopyFrom(NesterStateTransition<TGraph, TMacro> source)
	{
		CopyFrom((GraphElement<StateGraph>)source);
		nest = source.nest;
	}

	public abstract TGraph DefaultGraph();

	IGraph IGraphParent.DefaultGraph()
	{
		return DefaultGraph();
	}

	void IGraphNester.InstantiateNest()
	{
		InstantiateNest();
	}

	void IGraphNester.UninstantiateNest()
	{
		UninstantiateNest();
	}
}
