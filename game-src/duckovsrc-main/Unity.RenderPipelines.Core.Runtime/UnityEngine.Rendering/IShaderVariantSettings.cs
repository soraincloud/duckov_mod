namespace UnityEngine.Rendering;

public interface IShaderVariantSettings
{
	ShaderVariantLogLevel shaderVariantLogLevel { get; set; }

	bool exportShaderVariants { get; set; }
}
