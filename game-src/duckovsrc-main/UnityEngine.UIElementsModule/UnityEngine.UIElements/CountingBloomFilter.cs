namespace UnityEngine.UIElements;

internal struct CountingBloomFilter
{
	private const int KEY_SIZE = 14;

	private const uint ARRAY_SIZE = 16384u;

	private const int KEY_MASK = 16383;

	private unsafe fixed byte m_Counters[16384];

	private unsafe void AdjustSlot(uint index, bool increment)
	{
		if (increment)
		{
			if (m_Counters[index] != byte.MaxValue)
			{
				ref byte reference = ref m_Counters[index];
				reference++;
			}
		}
		else if (m_Counters[index] != 0)
		{
			ref byte reference2 = ref m_Counters[index];
			reference2--;
		}
	}

	private uint Hash1(uint hash)
	{
		return hash & 0x3FFF;
	}

	private uint Hash2(uint hash)
	{
		return (hash >> 14) & 0x3FFF;
	}

	private unsafe bool IsSlotEmpty(uint index)
	{
		return m_Counters[index] == 0;
	}

	public void InsertHash(uint hash)
	{
		AdjustSlot(Hash1(hash), increment: true);
		AdjustSlot(Hash2(hash), increment: true);
	}

	public void RemoveHash(uint hash)
	{
		AdjustSlot(Hash1(hash), increment: false);
		AdjustSlot(Hash2(hash), increment: false);
	}

	public bool ContainsHash(uint hash)
	{
		return !IsSlotEmpty(Hash1(hash)) && !IsSlotEmpty(Hash2(hash));
	}
}
