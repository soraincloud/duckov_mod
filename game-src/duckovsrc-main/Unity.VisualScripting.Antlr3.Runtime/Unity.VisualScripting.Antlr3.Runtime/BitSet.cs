using System;
using System.Collections;
using System.Text;

namespace Unity.VisualScripting.Antlr3.Runtime;

public class BitSet
{
	protected internal const int BITS = 64;

	protected internal const int LOG_BITS = 6;

	protected internal static readonly int MOD_MASK = 63;

	protected internal ulong[] bits;

	public virtual bool Nil
	{
		get
		{
			for (int num = bits.Length - 1; num >= 0; num--)
			{
				if (bits[num] != 0)
				{
					return false;
				}
			}
			return true;
		}
	}

	public virtual int Count
	{
		get
		{
			int num = 0;
			for (int num2 = bits.Length - 1; num2 >= 0; num2--)
			{
				ulong num3 = bits[num2];
				if (num3 != 0)
				{
					for (int num4 = 63; num4 >= 0; num4--)
					{
						if ((num3 & (ulong)(1L << num4)) != 0)
						{
							num++;
						}
					}
				}
			}
			return num;
		}
	}

	public BitSet()
		: this(64)
	{
	}

	public BitSet(ulong[] bits_)
	{
		bits = bits_;
	}

	public BitSet(IList items)
		: this(64)
	{
		for (int i = 0; i < items.Count; i++)
		{
			int el = (int)items[i];
			Add(el);
		}
	}

	public BitSet(int nbits)
	{
		bits = new ulong[(nbits - 1 >> 6) + 1];
	}

	public static BitSet Of(int el)
	{
		BitSet bitSet = new BitSet(el + 1);
		bitSet.Add(el);
		return bitSet;
	}

	public static BitSet Of(int a, int b)
	{
		BitSet bitSet = new BitSet(Math.Max(a, b) + 1);
		bitSet.Add(a);
		bitSet.Add(b);
		return bitSet;
	}

	public static BitSet Of(int a, int b, int c)
	{
		BitSet bitSet = new BitSet();
		bitSet.Add(a);
		bitSet.Add(b);
		bitSet.Add(c);
		return bitSet;
	}

	public static BitSet Of(int a, int b, int c, int d)
	{
		BitSet bitSet = new BitSet();
		bitSet.Add(a);
		bitSet.Add(b);
		bitSet.Add(c);
		bitSet.Add(d);
		return bitSet;
	}

	public virtual BitSet Or(BitSet a)
	{
		if (a == null)
		{
			return this;
		}
		BitSet bitSet = (BitSet)Clone();
		bitSet.OrInPlace(a);
		return bitSet;
	}

	public virtual void Add(int el)
	{
		int num = WordNumber(el);
		if (num >= bits.Length)
		{
			GrowToInclude(el);
		}
		bits[num] |= BitMask(el);
	}

	public virtual void GrowToInclude(int bit)
	{
		int num = Math.Max(bits.Length << 1, NumWordsToHold(bit));
		ulong[] destinationArray = new ulong[num];
		Array.Copy(bits, 0, destinationArray, 0, bits.Length);
		bits = destinationArray;
	}

	public virtual void OrInPlace(BitSet a)
	{
		if (a != null)
		{
			if (a.bits.Length > bits.Length)
			{
				SetSize(a.bits.Length);
			}
			int num = Math.Min(bits.Length, a.bits.Length);
			for (int num2 = num - 1; num2 >= 0; num2--)
			{
				bits[num2] |= a.bits[num2];
			}
		}
	}

	public virtual object Clone()
	{
		try
		{
			BitSet bitSet = (BitSet)MemberwiseClone();
			bitSet.bits = new ulong[bits.Length];
			Array.Copy(bits, 0, bitSet.bits, 0, bits.Length);
			return bitSet;
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException("Unable to clone BitSet", innerException);
		}
	}

	public virtual bool Member(int el)
	{
		if (el < 0)
		{
			return false;
		}
		int num = WordNumber(el);
		if (num >= bits.Length)
		{
			return false;
		}
		return (bits[num] & BitMask(el)) != 0;
	}

	public virtual void Remove(int el)
	{
		int num = WordNumber(el);
		if (num < bits.Length)
		{
			bits[num] &= ~BitMask(el);
		}
	}

	public virtual int NumBits()
	{
		return bits.Length << 6;
	}

	public virtual int LengthInLongWords()
	{
		return bits.Length;
	}

	public virtual int[] ToArray()
	{
		int[] array = new int[Count];
		int num = 0;
		for (int i = 0; i < bits.Length << 6; i++)
		{
			if (Member(i))
			{
				array[num++] = i;
			}
		}
		return array;
	}

	public virtual ulong[] ToPackedArray()
	{
		return bits;
	}

	private static int WordNumber(int bit)
	{
		return bit >> 6;
	}

	public override string ToString()
	{
		return ToString(null);
	}

	public virtual string ToString(string[] tokenNames)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string value = ",";
		bool flag = false;
		stringBuilder.Append('{');
		for (int i = 0; i < bits.Length << 6; i++)
		{
			if (Member(i))
			{
				if (i > 0 && flag)
				{
					stringBuilder.Append(value);
				}
				if (tokenNames != null)
				{
					stringBuilder.Append(tokenNames[i]);
				}
				else
				{
					stringBuilder.Append(i);
				}
				flag = true;
			}
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}

	public override bool Equals(object other)
	{
		if (other == null || !(other is BitSet))
		{
			return false;
		}
		BitSet bitSet = (BitSet)other;
		int num = Math.Min(bits.Length, bitSet.bits.Length);
		for (int i = 0; i < num; i++)
		{
			if (bits[i] != bitSet.bits[i])
			{
				return false;
			}
		}
		if (bits.Length > num)
		{
			for (int j = num + 1; j < bits.Length; j++)
			{
				if (bits[j] != 0)
				{
					return false;
				}
			}
		}
		else if (bitSet.bits.Length > num)
		{
			for (int k = num + 1; k < bitSet.bits.Length; k++)
			{
				if (bitSet.bits[k] != 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	private static ulong BitMask(int bitNumber)
	{
		int num = bitNumber & MOD_MASK;
		return (ulong)(1L << num);
	}

	private void SetSize(int nwords)
	{
		ulong[] destinationArray = new ulong[nwords];
		int length = Math.Min(nwords, bits.Length);
		Array.Copy(bits, 0, destinationArray, 0, length);
		bits = destinationArray;
	}

	private int NumWordsToHold(int el)
	{
		return (el >> 6) + 1;
	}
}
