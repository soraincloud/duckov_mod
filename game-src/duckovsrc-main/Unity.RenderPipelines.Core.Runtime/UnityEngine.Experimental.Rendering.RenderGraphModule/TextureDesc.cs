using System;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.RenderGraphModule;

public struct TextureDesc
{
	public TextureSizeMode sizeMode;

	public int width;

	public int height;

	public int slices;

	public Vector2 scale;

	public ScaleFunc func;

	public DepthBits depthBufferBits;

	public GraphicsFormat colorFormat;

	public FilterMode filterMode;

	public TextureWrapMode wrapMode;

	public TextureDimension dimension;

	public bool enableRandomWrite;

	public bool useMipMap;

	public bool autoGenerateMips;

	public bool isShadowMap;

	public int anisoLevel;

	public float mipMapBias;

	public MSAASamples msaaSamples;

	public bool bindTextureMS;

	public bool useDynamicScale;

	public RenderTextureMemoryless memoryless;

	public VRTextureUsage vrUsage;

	public string name;

	public FastMemoryDesc fastMemoryDesc;

	public bool fallBackToBlackTexture;

	public bool disableFallBackToImportedTexture;

	public bool clearBuffer;

	public Color clearColor;

	private void InitDefaultValues(bool dynamicResolution, bool xrReady)
	{
		useDynamicScale = dynamicResolution;
		vrUsage = VRTextureUsage.None;
		if (xrReady)
		{
			slices = TextureXR.slices;
			dimension = TextureXR.dimension;
		}
		else
		{
			slices = 1;
			dimension = TextureDimension.Tex2D;
		}
	}

	public TextureDesc(int width, int height, bool dynamicResolution = false, bool xrReady = false)
	{
		this = default(TextureDesc);
		sizeMode = TextureSizeMode.Explicit;
		this.width = width;
		this.height = height;
		msaaSamples = MSAASamples.None;
		InitDefaultValues(dynamicResolution, xrReady);
	}

	public TextureDesc(Vector2 scale, bool dynamicResolution = false, bool xrReady = false)
	{
		this = default(TextureDesc);
		sizeMode = TextureSizeMode.Scale;
		this.scale = scale;
		msaaSamples = MSAASamples.None;
		dimension = TextureDimension.Tex2D;
		InitDefaultValues(dynamicResolution, xrReady);
	}

	public TextureDesc(ScaleFunc func, bool dynamicResolution = false, bool xrReady = false)
	{
		this = default(TextureDesc);
		sizeMode = TextureSizeMode.Functor;
		this.func = func;
		msaaSamples = MSAASamples.None;
		dimension = TextureDimension.Tex2D;
		InitDefaultValues(dynamicResolution, xrReady);
	}

	public TextureDesc(TextureDesc input)
	{
		this = input;
	}

	public override int GetHashCode()
	{
		HashFNV1A32 hashFNV1A = HashFNV1A32.Create();
		switch (sizeMode)
		{
		case TextureSizeMode.Explicit:
			hashFNV1A.Append(in width);
			hashFNV1A.Append(in height);
			break;
		case TextureSizeMode.Functor:
			if (func != null)
			{
				hashFNV1A.Append((Delegate)func);
			}
			break;
		case TextureSizeMode.Scale:
			hashFNV1A.Append(in scale);
			break;
		}
		hashFNV1A.Append(in mipMapBias);
		hashFNV1A.Append(in slices);
		int input = (int)depthBufferBits;
		hashFNV1A.Append(in input);
		input = (int)colorFormat;
		hashFNV1A.Append(in input);
		input = (int)filterMode;
		hashFNV1A.Append(in input);
		input = (int)wrapMode;
		hashFNV1A.Append(in input);
		input = (int)dimension;
		hashFNV1A.Append(in input);
		input = (int)memoryless;
		hashFNV1A.Append(in input);
		input = (int)vrUsage;
		hashFNV1A.Append(in input);
		hashFNV1A.Append(in anisoLevel);
		hashFNV1A.Append(in enableRandomWrite);
		hashFNV1A.Append(in useMipMap);
		hashFNV1A.Append(in autoGenerateMips);
		hashFNV1A.Append(in isShadowMap);
		hashFNV1A.Append(in bindTextureMS);
		hashFNV1A.Append(in useDynamicScale);
		input = (int)msaaSamples;
		hashFNV1A.Append(in input);
		hashFNV1A.Append(in fastMemoryDesc.inFastMemory);
		return hashFNV1A.value;
	}
}
