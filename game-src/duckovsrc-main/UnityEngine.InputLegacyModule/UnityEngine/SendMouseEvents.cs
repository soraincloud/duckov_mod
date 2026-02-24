using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace UnityEngine;

internal class SendMouseEvents
{
	private struct HitInfo
	{
		public GameObject target;

		public Camera camera;

		public void SendMessage(string name)
		{
			target.SendMessage(name, null, SendMessageOptions.DontRequireReceiver);
		}

		public static implicit operator bool(HitInfo exists)
		{
			return exists.target != null && exists.camera != null;
		}

		public static bool Compare(HitInfo lhs, HitInfo rhs)
		{
			return lhs.target == rhs.target && lhs.camera == rhs.camera;
		}
	}

	public enum LeftMouseButtonState
	{
		NotPressed,
		Pressed,
		PressedThisFrame
	}

	private const int m_HitIndexGUI = 0;

	private const int m_HitIndexPhysics3D = 1;

	private const int m_HitIndexPhysics2D = 2;

	private static bool s_MouseUsed = false;

	private static readonly HitInfo[] m_LastHit = new HitInfo[3];

	private static readonly HitInfo[] m_MouseDownHit = new HitInfo[3];

	private static readonly HitInfo[] m_CurrentHit = new HitInfo[3];

	private static Camera[] m_Cameras;

	public static Func<KeyValuePair<int, Vector2>> s_GetMouseState;

	private static Vector2 s_MousePosition;

	private static bool s_MouseButtonPressedThisFrame;

	private static bool s_MouseButtonIsPressed;

	private static void UpdateMouse()
	{
		if (s_GetMouseState != null)
		{
			KeyValuePair<int, Vector2> keyValuePair = s_GetMouseState();
			s_MousePosition = keyValuePair.Value;
			s_MouseButtonPressedThisFrame = keyValuePair.Key == 2;
			s_MouseButtonIsPressed = keyValuePair.Key != 0;
		}
		else if (!Input.CheckDisabled())
		{
			s_MousePosition = Input.mousePosition;
			s_MouseButtonPressedThisFrame = Input.GetMouseButtonDown(0);
			s_MouseButtonIsPressed = Input.GetMouseButton(0);
		}
		else
		{
			s_MousePosition = default(Vector2);
			s_MouseButtonPressedThisFrame = false;
			s_MouseButtonIsPressed = false;
		}
	}

	[RequiredByNativeCode]
	private static void SetMouseMoved()
	{
		s_MouseUsed = true;
	}

	[RequiredByNativeCode]
	private static void DoSendMouseEvents(int skipRTCameras)
	{
		UpdateMouse();
		Vector2 vector = s_MousePosition;
		int allCamerasCount = Camera.allCamerasCount;
		if (m_Cameras == null || m_Cameras.Length != allCamerasCount)
		{
			m_Cameras = new Camera[allCamerasCount];
		}
		Camera.GetAllCameras(m_Cameras);
		for (int i = 0; i < m_CurrentHit.Length; i++)
		{
			m_CurrentHit[i] = default(HitInfo);
		}
		if (!s_MouseUsed)
		{
			Camera[] cameras = m_Cameras;
			foreach (Camera camera in cameras)
			{
				if (camera == null || (skipRTCameras != 0 && camera.targetTexture != null))
				{
					continue;
				}
				int targetDisplay = camera.targetDisplay;
				Vector3 vector2 = Display.RelativeMouseAt(vector);
				if (vector2 != Vector3.zero)
				{
					int num = (int)vector2.z;
					if (num != targetDisplay)
					{
						continue;
					}
					float num2 = Screen.width;
					float num3 = Screen.height;
					if (targetDisplay > 0 && targetDisplay < Display.displays.Length)
					{
						num2 = Display.displays[targetDisplay].systemWidth;
						num3 = Display.displays[targetDisplay].systemHeight;
					}
					Vector2 vector3 = new Vector2(vector2.x / num2, vector2.y / num3);
					if (vector3.x < 0f || vector3.x > 1f || vector3.y < 0f || vector3.y > 1f)
					{
						continue;
					}
				}
				else
				{
					vector2 = vector;
				}
				if (camera.pixelRect.Contains(vector2) && camera.eventMask != 0)
				{
					Ray ray = camera.ScreenPointToRay(vector2);
					float z = ray.direction.z;
					float distance = (Mathf.Approximately(0f, z) ? float.PositiveInfinity : Mathf.Abs((camera.farClipPlane - camera.nearClipPlane) / z));
					GameObject gameObject = CameraRaycastHelper.RaycastTry(camera, ray, distance, camera.cullingMask & camera.eventMask);
					if (gameObject != null)
					{
						m_CurrentHit[1].target = gameObject;
						m_CurrentHit[1].camera = camera;
					}
					else if (camera.clearFlags == CameraClearFlags.Skybox || camera.clearFlags == CameraClearFlags.Color)
					{
						m_CurrentHit[1].target = null;
						m_CurrentHit[1].camera = null;
					}
					GameObject gameObject2 = CameraRaycastHelper.RaycastTry2D(camera, ray, distance, camera.cullingMask & camera.eventMask);
					if (gameObject2 != null)
					{
						m_CurrentHit[2].target = gameObject2;
						m_CurrentHit[2].camera = camera;
					}
					else if (camera.clearFlags == CameraClearFlags.Skybox || camera.clearFlags == CameraClearFlags.Color)
					{
						m_CurrentHit[2].target = null;
						m_CurrentHit[2].camera = null;
					}
				}
			}
		}
		for (int k = 0; k < m_CurrentHit.Length; k++)
		{
			SendEvents(k, m_CurrentHit[k]);
		}
		s_MouseUsed = false;
	}

	private static void SendEvents(int i, HitInfo hit)
	{
		bool flag = s_MouseButtonPressedThisFrame;
		bool flag2 = s_MouseButtonIsPressed;
		if (flag)
		{
			if ((bool)hit)
			{
				m_MouseDownHit[i] = hit;
				m_MouseDownHit[i].SendMessage("OnMouseDown");
			}
		}
		else if (!flag2)
		{
			if ((bool)m_MouseDownHit[i])
			{
				if (HitInfo.Compare(hit, m_MouseDownHit[i]))
				{
					m_MouseDownHit[i].SendMessage("OnMouseUpAsButton");
				}
				m_MouseDownHit[i].SendMessage("OnMouseUp");
				m_MouseDownHit[i] = default(HitInfo);
			}
		}
		else if ((bool)m_MouseDownHit[i])
		{
			m_MouseDownHit[i].SendMessage("OnMouseDrag");
		}
		if (HitInfo.Compare(hit, m_LastHit[i]))
		{
			if ((bool)hit)
			{
				hit.SendMessage("OnMouseOver");
			}
		}
		else
		{
			if ((bool)m_LastHit[i])
			{
				m_LastHit[i].SendMessage("OnMouseExit");
			}
			if ((bool)hit)
			{
				hit.SendMessage("OnMouseEnter");
				hit.SendMessage("OnMouseOver");
			}
		}
		m_LastHit[i] = hit;
	}
}
