using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;

namespace UnityEngine.Analytics;

[StructLayout(LayoutKind.Sequential)]
[NativeHeader("Modules/UnityAnalytics/Public/Events/UserCustomEvent.h")]
internal class CustomEventData : IDisposable
{
	[NonSerialized]
	internal IntPtr m_Ptr;

	private CustomEventData()
	{
	}

	public CustomEventData(string name)
	{
		m_Ptr = Internal_Create(this, name);
	}

	~CustomEventData()
	{
		Destroy();
	}

	private void Destroy()
	{
		if (m_Ptr != IntPtr.Zero)
		{
			Internal_Destroy(m_Ptr);
			m_Ptr = IntPtr.Zero;
		}
	}

	public void Dispose()
	{
		Destroy();
		GC.SuppressFinalize(this);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern IntPtr Internal_Create(CustomEventData ced, string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ThreadSafe]
	internal static extern void Internal_Destroy(IntPtr ptr);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern bool AddString(string key, string value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern bool AddInt32(string key, int value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern bool AddUInt32(string key, uint value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern bool AddInt64(string key, long value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern bool AddUInt64(string key, ulong value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern bool AddBool(string key, bool value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern bool AddDouble(string key, double value);

	public bool AddDictionary(IDictionary<string, object> eventData)
	{
		foreach (KeyValuePair<string, object> eventDatum in eventData)
		{
			string key = eventDatum.Key;
			object value = eventDatum.Value;
			if (value == null)
			{
				AddString(key, "null");
				continue;
			}
			Type type = value.GetType();
			if (type == typeof(string))
			{
				AddString(key, (string)value);
				continue;
			}
			if (type == typeof(char))
			{
				AddString(key, char.ToString((char)value));
				continue;
			}
			if (type == typeof(sbyte))
			{
				AddInt32(key, (sbyte)value);
				continue;
			}
			if (type == typeof(byte))
			{
				AddInt32(key, (byte)value);
				continue;
			}
			if (type == typeof(short))
			{
				AddInt32(key, (short)value);
				continue;
			}
			if (type == typeof(ushort))
			{
				AddUInt32(key, (ushort)value);
				continue;
			}
			if (type == typeof(int))
			{
				AddInt32(key, (int)value);
				continue;
			}
			if (type == typeof(uint))
			{
				AddUInt32(eventDatum.Key, (uint)value);
				continue;
			}
			if (type == typeof(long))
			{
				AddInt64(key, (long)value);
				continue;
			}
			if (type == typeof(ulong))
			{
				AddUInt64(key, (ulong)value);
				continue;
			}
			if (type == typeof(bool))
			{
				AddBool(key, (bool)value);
				continue;
			}
			if (type == typeof(float))
			{
				AddDouble(key, (double)Convert.ToDecimal((float)value));
				continue;
			}
			if (type == typeof(double))
			{
				AddDouble(key, (double)value);
				continue;
			}
			if (type == typeof(decimal))
			{
				AddDouble(key, (double)Convert.ToDecimal((decimal)value));
				continue;
			}
			if (type.IsValueType)
			{
				AddString(key, value.ToString());
				continue;
			}
			throw new ArgumentException($"Invalid type: {type} passed");
		}
		return true;
	}
}
