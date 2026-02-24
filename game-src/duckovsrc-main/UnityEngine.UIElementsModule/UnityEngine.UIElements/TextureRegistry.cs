#define UNITY_ASSERTIONS
using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class TextureRegistry
{
	private struct TextureInfo
	{
		public Texture texture;

		public bool dynamic;

		public int refCount;
	}

	public struct Statistics
	{
		public int freeIdsCount;

		public int createdIdsCount;

		public int allocatedIdsTotalCount;

		public int allocatedIdsDynamicCount;

		public int allocatedIdsStaticCount;

		public int availableIdsCount;
	}

	private List<TextureInfo> m_Textures = new List<TextureInfo>(128);

	private Dictionary<Texture, TextureId> m_TextureToId = new Dictionary<Texture, TextureId>(128);

	private Stack<TextureId> m_FreeIds = new Stack<TextureId>();

	internal const int maxTextures = 2048;

	public static TextureRegistry instance { get; } = new TextureRegistry();

	public Texture GetTexture(TextureId id)
	{
		if (id.index < 0 || id.index >= m_Textures.Count)
		{
			Debug.LogError($"Attempted to get an invalid texture (index={id.index}).");
			return null;
		}
		TextureInfo textureInfo = m_Textures[id.index];
		if (textureInfo.refCount < 1)
		{
			Debug.LogError($"Attempted to get a texture (index={id.index}) that is not allocated.");
			return null;
		}
		return textureInfo.texture;
	}

	public TextureId AllocAndAcquireDynamic()
	{
		return AllocAndAcquire(null, dynamic: true);
	}

	public void UpdateDynamic(TextureId id, Texture texture)
	{
		if (id.index < 0 || id.index >= m_Textures.Count)
		{
			Debug.LogError($"Attempted to update an invalid dynamic texture (index={id.index}).");
			return;
		}
		TextureInfo value = m_Textures[id.index];
		if (!value.dynamic)
		{
			Debug.LogError($"Attempted to update a texture (index={id.index}) that is not dynamic.");
			return;
		}
		if (value.refCount < 1)
		{
			Debug.LogError($"Attempted to update a dynamic texture (index={id.index}) that is not allocated.");
			return;
		}
		value.texture = texture;
		m_Textures[id.index] = value;
	}

	private TextureId AllocAndAcquire(Texture texture, bool dynamic)
	{
		TextureInfo textureInfo = new TextureInfo
		{
			texture = texture,
			dynamic = dynamic,
			refCount = 1
		};
		TextureId textureId;
		if (m_FreeIds.Count > 0)
		{
			textureId = m_FreeIds.Pop();
			m_Textures[textureId.index] = textureInfo;
		}
		else
		{
			if (m_Textures.Count == 2048)
			{
				Debug.LogError(string.Format("Failed to allocate a {0} because the limit of {1} textures is reached.", "TextureId", 2048));
				return TextureId.invalid;
			}
			textureId = new TextureId(m_Textures.Count);
			m_Textures.Add(textureInfo);
		}
		if (!dynamic)
		{
			m_TextureToId[texture] = textureId;
		}
		return textureId;
	}

	public TextureId Acquire(Texture tex)
	{
		if (m_TextureToId.TryGetValue(tex, out var value))
		{
			TextureInfo value2 = m_Textures[value.index];
			Debug.Assert(value2.refCount > 0);
			Debug.Assert(!value2.dynamic);
			value2.refCount++;
			m_Textures[value.index] = value2;
			return value;
		}
		return AllocAndAcquire(tex, dynamic: false);
	}

	public void Acquire(TextureId id)
	{
		if (id.index < 0 || id.index >= m_Textures.Count)
		{
			Debug.LogError($"Attempted to acquire an invalid texture (index={id.index}).");
			return;
		}
		TextureInfo value = m_Textures[id.index];
		if (value.refCount < 1)
		{
			Debug.LogError($"Attempted to acquire a texture (index={id.index}) that is not allocated.");
			return;
		}
		value.refCount++;
		m_Textures[id.index] = value;
	}

	public void Release(TextureId id)
	{
		if (id.index < 0 || id.index >= m_Textures.Count)
		{
			Debug.LogError($"Attempted to release an invalid texture (index={id.index}).");
			return;
		}
		TextureInfo value = m_Textures[id.index];
		if (value.refCount < 1)
		{
			Debug.LogError($"Attempted to release a texture (index={id.index}) that is not allocated.");
			return;
		}
		value.refCount--;
		if (value.refCount == 0)
		{
			if (!value.dynamic)
			{
				m_TextureToId.Remove(value.texture);
			}
			value.texture = null;
			value.dynamic = false;
			m_FreeIds.Push(id);
		}
		m_Textures[id.index] = value;
	}

	public TextureId TextureToId(Texture texture)
	{
		if (m_TextureToId.TryGetValue(texture, out var value))
		{
			return value;
		}
		return TextureId.invalid;
	}

	public Statistics GatherStatistics()
	{
		Statistics result = default(Statistics);
		result.freeIdsCount = m_FreeIds.Count;
		result.createdIdsCount = m_Textures.Count;
		result.allocatedIdsTotalCount = m_Textures.Count - m_FreeIds.Count;
		result.allocatedIdsDynamicCount = result.allocatedIdsTotalCount - m_TextureToId.Count;
		result.allocatedIdsStaticCount = result.allocatedIdsTotalCount - result.allocatedIdsDynamicCount;
		result.availableIdsCount = 2048 - result.allocatedIdsTotalCount;
		return result;
	}
}
