namespace Unity.Profiling;

internal sealed class ProfilerRecorderDebugView
{
	private ProfilerRecorder m_Recorder;

	public ProfilerRecorderSample[] Items => m_Recorder.ToArray();

	public ProfilerRecorderDebugView(ProfilerRecorder r)
	{
		m_Recorder = r;
	}
}
