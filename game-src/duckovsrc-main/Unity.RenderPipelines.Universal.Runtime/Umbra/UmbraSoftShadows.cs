using System;
using UnityEngine;

namespace Umbra;

[ExecuteAlways]
[HelpURL("https://kronnect.com/guides-category/umbra-soft-shadows")]
public class UmbraSoftShadows : MonoBehaviour
{
	[Tooltip("Currently used umbra profile with settings")]
	public UmbraProfile profile;

	public bool debugShadows;

	public static bool installed;

	public static bool isDeferred;

	public static bool softShadowOn;

	public static event Action OnSettingChangedEvent;

	public static void InvokeOnSettingChangedEvent()
	{
		UmbraSoftShadows.OnSettingChangedEvent?.Invoke();
	}

	private void Awake()
	{
		OnSettingChangedEvent += OnSettingChanged;
	}

	private void OnDestroy()
	{
		OnSettingChangedEvent -= OnSettingChanged;
	}

	private void OnSettingChanged()
	{
		base.enabled = softShadowOn;
	}

	private void OnEnable()
	{
		CheckProfile();
	}

	private void OnDisable()
	{
		UmbraRenderFeature.UnregisterUmbraLight(this);
	}

	private void OnValidate()
	{
		CheckProfile();
	}

	private void Reset()
	{
		CheckProfile();
	}

	private void CheckProfile()
	{
		if (profile == null)
		{
			profile = ScriptableObject.CreateInstance<UmbraProfile>();
			profile.name = "New Umbra Profile";
		}
		UmbraRenderFeature.RegisterUmbraLight(this);
	}
}
