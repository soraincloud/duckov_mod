namespace UnityEngine.Rendering.Universal;

internal sealed class MotionVectorsPersistentData
{
	private const int k_EyeCount = 2;

	private readonly Matrix4x4[] m_ViewProjection = new Matrix4x4[2];

	private readonly Matrix4x4[] m_PreviousViewProjection = new Matrix4x4[2];

	private readonly int[] m_LastFrameIndex = new int[2];

	private readonly float[] m_PrevAspectRatio = new float[2];

	internal int lastFrameIndex => m_LastFrameIndex[0];

	internal Matrix4x4 viewProjection => m_ViewProjection[0];

	internal Matrix4x4 previousViewProjection => m_PreviousViewProjection[0];

	internal Matrix4x4[] viewProjectionStereo => m_ViewProjection;

	internal Matrix4x4[] previousViewProjectionStereo => m_PreviousViewProjection;

	internal MotionVectorsPersistentData()
	{
		Reset();
	}

	public void Reset()
	{
		for (int i = 0; i < m_ViewProjection.Length; i++)
		{
			m_ViewProjection[i] = Matrix4x4.identity;
			m_PreviousViewProjection[i] = Matrix4x4.identity;
			m_LastFrameIndex[i] = -1;
			m_PrevAspectRatio[i] = -1f;
		}
	}

	internal int GetXRMultiPassId(ref CameraData cameraData)
	{
		if (!cameraData.xr.enabled)
		{
			return 0;
		}
		return cameraData.xr.multipassId;
	}

	public void Update(ref CameraData cameraData)
	{
		int xRMultiPassId = GetXRMultiPassId(ref cameraData);
		bool flag = m_PrevAspectRatio[xRMultiPassId] != cameraData.aspectRatio;
		if (m_LastFrameIndex[xRMultiPassId] != Time.frameCount || flag)
		{
			if (cameraData.xr.enabled && cameraData.xr.singlePassEnabled)
			{
				Matrix4x4 matrix4x = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrixNoJitter(), renderIntoTexture: true) * cameraData.GetViewMatrix();
				Matrix4x4 matrix4x2 = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrixNoJitter(1), renderIntoTexture: true) * cameraData.GetViewMatrix(1);
				m_PreviousViewProjection[0] = (flag ? matrix4x : m_ViewProjection[0]);
				m_PreviousViewProjection[1] = (flag ? matrix4x2 : m_ViewProjection[1]);
				m_ViewProjection[0] = matrix4x;
				m_ViewProjection[1] = matrix4x2;
			}
			else
			{
				Matrix4x4 matrix4x3 = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrixNoJitter(), renderIntoTexture: true) * cameraData.GetViewMatrix();
				m_PreviousViewProjection[xRMultiPassId] = (flag ? matrix4x3 : m_ViewProjection[xRMultiPassId]);
				m_ViewProjection[xRMultiPassId] = matrix4x3;
			}
			m_LastFrameIndex[xRMultiPassId] = Time.frameCount;
			m_PrevAspectRatio[xRMultiPassId] = cameraData.aspectRatio;
		}
	}
}
