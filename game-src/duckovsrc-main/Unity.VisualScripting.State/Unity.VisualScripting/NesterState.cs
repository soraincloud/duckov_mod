using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting;

public abstract class NesterState<TGraph, TMacro> : State, INesterState, IState, IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable, IGraphElementWithData, IGraphNesterElement, IGraphParentElement, IGraphParent, IGraphNester where TGraph : class, IGraph, new() where TMacro : Macro<TGraph>
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

	protected NesterState()
	{
		nest.nester = this;
	}

	protected NesterState(TMacro macro)
	{
		nest.nester = this;
		nest.macro = macro;
		nest.source = GraphSource.Macro;
	}

	protected void CopyFrom(NesterState<TGraph, TMacro> source)
	{
		CopyFrom((State)source);
		nest = source.nest;
	}

	public override IEnumerable<object> GetAotStubs(HashSet<object> visited)
	{
		return LinqUtility.Concat<object>(new IEnumerable[2]
		{
			base.GetAotStubs(visited),
			nest.GetAotStubs(visited)
		});
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

	StateGraph IState.get_graph()
	{
		return base.graph;
	}
}
