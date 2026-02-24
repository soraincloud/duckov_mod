using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering;

public interface IVolumeDebugSettings2 : IVolumeDebugSettings
{
	Type targetRenderPipeline { get; }

	List<(string, Type)> volumeComponentsPathAndType { get; }
}
