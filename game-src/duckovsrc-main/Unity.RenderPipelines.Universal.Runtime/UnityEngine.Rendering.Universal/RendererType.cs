using System;

namespace UnityEngine.Rendering.Universal;

public enum RendererType
{
	Custom = 0,
	UniversalRenderer = 1,
	_2DRenderer = 2,
	[Obsolete("ForwardRenderer has been renamed (UnityUpgradable) -> UniversalRenderer", true)]
	ForwardRenderer = 1
}
