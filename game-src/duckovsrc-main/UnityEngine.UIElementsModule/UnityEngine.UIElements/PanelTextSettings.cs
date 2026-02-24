using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.TextCore.Text;

namespace UnityEngine.UIElements;

public class PanelTextSettings : TextSettings
{
	private static PanelTextSettings s_DefaultPanelTextSettings;

	internal static Func<string, Object> EditorGUIUtilityLoad;

	internal static Func<SystemLanguage> GetCurrentLanguage;

	internal static readonly string s_DefaultEditorPanelTextSettingPath = "UIPackageResources/Default Editor Text Settings.asset";

	internal static PanelTextSettings defaultPanelTextSettings
	{
		get
		{
			if (s_DefaultPanelTextSettings == null && s_DefaultPanelTextSettings == null)
			{
				s_DefaultPanelTextSettings = ScriptableObject.CreateInstance<PanelTextSettings>();
			}
			return s_DefaultPanelTextSettings;
		}
	}

	internal static void UpdateLocalizationFontAsset()
	{
		string text = " - Linux";
		Dictionary<SystemLanguage, string> dictionary = new Dictionary<SystemLanguage, string>
		{
			{
				SystemLanguage.English,
				Path.Combine(UIElementsPackageUtility.EditorResourcesBasePath, "UIPackageResources/FontAssets/DynamicOSFontAssets/Localization/English" + text + ".asset")
			},
			{
				SystemLanguage.Japanese,
				Path.Combine(UIElementsPackageUtility.EditorResourcesBasePath, "UIPackageResources/FontAssets/DynamicOSFontAssets/Localization/Japanese" + text + ".asset")
			},
			{
				SystemLanguage.ChineseSimplified,
				Path.Combine(UIElementsPackageUtility.EditorResourcesBasePath, "UIPackageResources/FontAssets/DynamicOSFontAssets/Localization/ChineseSimplified" + text + ".asset")
			},
			{
				SystemLanguage.ChineseTraditional,
				Path.Combine(UIElementsPackageUtility.EditorResourcesBasePath, "UIPackageResources/FontAssets/DynamicOSFontAssets/Localization/ChineseTraditional" + text + ".asset")
			},
			{
				SystemLanguage.Korean,
				Path.Combine(UIElementsPackageUtility.EditorResourcesBasePath, "UIPackageResources/FontAssets/DynamicOSFontAssets/Localization/Korean" + text + ".asset")
			}
		};
		string arg = Path.Combine(UIElementsPackageUtility.EditorResourcesBasePath, "UIPackageResources/FontAssets/DynamicOSFontAssets/GlobalFallback/GlobalFallback" + text + ".asset");
		FontAsset value = EditorGUIUtilityLoad(dictionary[GetCurrentLanguage()]) as FontAsset;
		FontAsset value2 = EditorGUIUtilityLoad(arg) as FontAsset;
		defaultPanelTextSettings.fallbackFontAssets[0] = value;
		defaultPanelTextSettings.fallbackFontAssets[defaultPanelTextSettings.fallbackFontAssets.Count - 1] = value2;
	}

	internal FontAsset GetCachedFontAsset(Font font)
	{
		return GetCachedFontAssetInternal(font);
	}
}
