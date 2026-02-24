using System;

namespace UnityEngine.Rendering.Universal;

public class UniversalRenderPipelineVolumeDebugSettings : VolumeDebugSettings<UniversalAdditionalCameraData>
{
	public override Type targetRenderPipeline => typeof(UniversalRenderPipeline);

	public override VolumeStack selectedCameraVolumeStack
	{
		get
		{
			if (base.selectedCamera == null)
			{
				return null;
			}
			UniversalAdditionalCameraData component = base.selectedCamera.GetComponent<UniversalAdditionalCameraData>();
			if (component == null)
			{
				return null;
			}
			VolumeStack volumeStack = component.volumeStack;
			if (volumeStack != null)
			{
				return volumeStack;
			}
			return VolumeManager.instance.stack;
		}
	}

	public override LayerMask selectedCameraLayerMask
	{
		get
		{
			if (!(base.selectedCamera != null))
			{
				return 0;
			}
			return base.selectedCamera.GetComponent<UniversalAdditionalCameraData>().volumeLayerMask;
		}
	}

	public override Vector3 selectedCameraPosition
	{
		get
		{
			if (!(base.selectedCamera != null))
			{
				return Vector3.zero;
			}
			return base.selectedCamera.transform.position;
		}
	}
}
