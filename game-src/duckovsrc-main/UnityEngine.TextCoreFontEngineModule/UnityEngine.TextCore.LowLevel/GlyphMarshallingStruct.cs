using UnityEngine.Scripting;

namespace UnityEngine.TextCore.LowLevel;

[UsedByNativeCode]
internal struct GlyphMarshallingStruct
{
	public uint index;

	public GlyphMetrics metrics;

	public GlyphRect glyphRect;

	public float scale;

	public int atlasIndex;

	public GlyphClassDefinitionType classDefinitionType;

	public GlyphMarshallingStruct(Glyph glyph)
	{
		index = glyph.index;
		metrics = glyph.metrics;
		glyphRect = glyph.glyphRect;
		scale = glyph.scale;
		atlasIndex = glyph.atlasIndex;
		classDefinitionType = glyph.classDefinitionType;
	}

	public GlyphMarshallingStruct(uint index, GlyphMetrics metrics, GlyphRect glyphRect, float scale, int atlasIndex)
	{
		this.index = index;
		this.metrics = metrics;
		this.glyphRect = glyphRect;
		this.scale = scale;
		this.atlasIndex = atlasIndex;
		classDefinitionType = GlyphClassDefinitionType.Undefined;
	}

	public GlyphMarshallingStruct(uint index, GlyphMetrics metrics, GlyphRect glyphRect, float scale, int atlasIndex, GlyphClassDefinitionType classDefinitionType)
	{
		this.index = index;
		this.metrics = metrics;
		this.glyphRect = glyphRect;
		this.scale = scale;
		this.atlasIndex = atlasIndex;
		this.classDefinitionType = classDefinitionType;
	}
}
