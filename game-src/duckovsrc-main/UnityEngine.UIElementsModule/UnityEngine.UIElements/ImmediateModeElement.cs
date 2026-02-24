using System;
using System.Collections.Generic;
using Unity.Profiling;

namespace UnityEngine.UIElements;

public abstract class ImmediateModeElement : VisualElement
{
	private static readonly Dictionary<Type, ProfilerMarker> s_Markers = new Dictionary<Type, ProfilerMarker>();

	private readonly ProfilerMarker m_ImmediateRepaintMarker;

	private bool m_CullingEnabled = false;

	public bool cullingEnabled
	{
		get
		{
			return m_CullingEnabled;
		}
		set
		{
			m_CullingEnabled = value;
			IncrementVersion(VersionChangeType.Repaint);
		}
	}

	public ImmediateModeElement()
	{
		base.generateVisualContent = (Action<MeshGenerationContext>)Delegate.Combine(base.generateVisualContent, new Action<MeshGenerationContext>(OnGenerateVisualContent));
		Type type = GetType();
		if (!s_Markers.TryGetValue(type, out m_ImmediateRepaintMarker))
		{
			m_ImmediateRepaintMarker = new ProfilerMarker(base.typeName + ".ImmediateRepaint");
			s_Markers[type] = m_ImmediateRepaintMarker;
		}
	}

	private void OnGenerateVisualContent(MeshGenerationContext mgc)
	{
		mgc.painter.DrawImmediate(CallImmediateRepaint, cullingEnabled);
	}

	private void CallImmediateRepaint()
	{
		using (m_ImmediateRepaintMarker.Auto())
		{
			ImmediateRepaint();
		}
	}

	protected abstract void ImmediateRepaint();
}
