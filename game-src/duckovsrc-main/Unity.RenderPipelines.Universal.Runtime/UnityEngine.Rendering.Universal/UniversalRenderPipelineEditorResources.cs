using System;

namespace UnityEngine.Rendering.Universal;

public class UniversalRenderPipelineEditorResources : ScriptableObject
{
	[Serializable]
	[ReloadGroup]
	public sealed class ShaderResources
	{
		[Reload("Shaders/AutodeskInteractive/AutodeskInteractive.shadergraph", ReloadAttribute.Package.Root)]
		public Shader autodeskInteractivePS;

		[Reload("Shaders/AutodeskInteractive/AutodeskInteractiveTransparent.shadergraph", ReloadAttribute.Package.Root)]
		public Shader autodeskInteractiveTransparentPS;

		[Reload("Shaders/AutodeskInteractive/AutodeskInteractiveMasked.shadergraph", ReloadAttribute.Package.Root)]
		public Shader autodeskInteractiveMaskedPS;

		[Reload("Shaders/Terrain/TerrainDetailLit.shader", ReloadAttribute.Package.Root)]
		public Shader terrainDetailLitPS;

		[Reload("Shaders/Terrain/WavingGrass.shader", ReloadAttribute.Package.Root)]
		public Shader terrainDetailGrassPS;

		[Reload("Shaders/Terrain/WavingGrassBillboard.shader", ReloadAttribute.Package.Root)]
		public Shader terrainDetailGrassBillboardPS;

		[Reload("Shaders/Nature/SpeedTree7.shader", ReloadAttribute.Package.Root)]
		public Shader defaultSpeedTree7PS;

		[Reload("Shaders/Nature/SpeedTree8_PBRLit.shadergraph", ReloadAttribute.Package.Root)]
		public Shader defaultSpeedTree8PS;
	}

	[Serializable]
	[ReloadGroup]
	public sealed class MaterialResources
	{
		[Reload("Runtime/Materials/Lit.mat", ReloadAttribute.Package.Root)]
		public Material lit;

		[Reload("Runtime/Materials/ParticlesUnlit.mat", ReloadAttribute.Package.Root)]
		public Material particleLit;

		[Reload("Runtime/Materials/TerrainLit.mat", ReloadAttribute.Package.Root)]
		public Material terrainLit;

		[Reload("Runtime/Materials/Decal.mat", ReloadAttribute.Package.Root)]
		public Material decal;
	}

	public ShaderResources shaders;

	public MaterialResources materials;
}
