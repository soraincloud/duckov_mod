using System.Collections.Generic;

namespace UnityEngine.TextCore.Text;

internal static class MaterialManager
{
	private static Dictionary<long, Material> s_FallbackMaterials = new Dictionary<long, Material>();

	public static Material GetFallbackMaterial(Material sourceMaterial, Material targetMaterial)
	{
		int instanceID = sourceMaterial.GetInstanceID();
		Texture texture = targetMaterial.GetTexture(TextShaderUtilities.ID_MainTex);
		int instanceID2 = texture.GetInstanceID();
		long key = ((long)instanceID << 32) | (uint)instanceID2;
		if (s_FallbackMaterials.TryGetValue(key, out var value))
		{
			int num = sourceMaterial.ComputeCRC();
			int num2 = value.ComputeCRC();
			if (num == num2)
			{
				return value;
			}
			CopyMaterialPresetProperties(sourceMaterial, value);
			return value;
		}
		if (sourceMaterial.HasProperty(TextShaderUtilities.ID_GradientScale) && targetMaterial.HasProperty(TextShaderUtilities.ID_GradientScale))
		{
			value = new Material(sourceMaterial);
			value.hideFlags = HideFlags.HideAndDontSave;
			value.SetTexture(TextShaderUtilities.ID_MainTex, texture);
			value.SetFloat(TextShaderUtilities.ID_GradientScale, targetMaterial.GetFloat(TextShaderUtilities.ID_GradientScale));
			value.SetFloat(TextShaderUtilities.ID_TextureWidth, targetMaterial.GetFloat(TextShaderUtilities.ID_TextureWidth));
			value.SetFloat(TextShaderUtilities.ID_TextureHeight, targetMaterial.GetFloat(TextShaderUtilities.ID_TextureHeight));
			value.SetFloat(TextShaderUtilities.ID_WeightNormal, targetMaterial.GetFloat(TextShaderUtilities.ID_WeightNormal));
			value.SetFloat(TextShaderUtilities.ID_WeightBold, targetMaterial.GetFloat(TextShaderUtilities.ID_WeightBold));
		}
		else
		{
			value = new Material(targetMaterial);
		}
		s_FallbackMaterials.Add(key, value);
		return value;
	}

	public static Material GetFallbackMaterial(FontAsset fontAsset, Material sourceMaterial, int atlasIndex)
	{
		int instanceID = sourceMaterial.GetInstanceID();
		Texture texture = fontAsset.atlasTextures[atlasIndex];
		int instanceID2 = texture.GetInstanceID();
		long key = ((long)instanceID << 32) | (uint)instanceID2;
		if (s_FallbackMaterials.TryGetValue(key, out var value))
		{
			int num = sourceMaterial.ComputeCRC();
			int num2 = value.ComputeCRC();
			if (num == num2)
			{
				return value;
			}
			CopyMaterialPresetProperties(sourceMaterial, value);
			return value;
		}
		value = new Material(sourceMaterial);
		value.SetTexture(TextShaderUtilities.ID_MainTex, texture);
		value.hideFlags = HideFlags.HideAndDontSave;
		s_FallbackMaterials.Add(key, value);
		return value;
	}

	private static void CopyMaterialPresetProperties(Material source, Material destination)
	{
		if (source.HasProperty(TextShaderUtilities.ID_GradientScale) && destination.HasProperty(TextShaderUtilities.ID_GradientScale))
		{
			Texture texture = destination.GetTexture(TextShaderUtilities.ID_MainTex);
			float value = destination.GetFloat(TextShaderUtilities.ID_GradientScale);
			float value2 = destination.GetFloat(TextShaderUtilities.ID_TextureWidth);
			float value3 = destination.GetFloat(TextShaderUtilities.ID_TextureHeight);
			float value4 = destination.GetFloat(TextShaderUtilities.ID_WeightNormal);
			float value5 = destination.GetFloat(TextShaderUtilities.ID_WeightBold);
			destination.shader = source.shader;
			destination.CopyPropertiesFromMaterial(source);
			destination.shaderKeywords = source.shaderKeywords;
			destination.SetTexture(TextShaderUtilities.ID_MainTex, texture);
			destination.SetFloat(TextShaderUtilities.ID_GradientScale, value);
			destination.SetFloat(TextShaderUtilities.ID_TextureWidth, value2);
			destination.SetFloat(TextShaderUtilities.ID_TextureHeight, value3);
			destination.SetFloat(TextShaderUtilities.ID_WeightNormal, value4);
			destination.SetFloat(TextShaderUtilities.ID_WeightBold, value5);
		}
	}
}
