using System;
using System.Globalization;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Unity.Properties;

public static class TypeConversion
{
	private static class PrimitiveConverters
	{
		public static void Register()
		{
			RegisterInt8Converters();
			RegisterInt16Converters();
			RegisterInt32Converters();
			RegisterInt64Converters();
			RegisterUInt8Converters();
			RegisterUInt16Converters();
			RegisterUInt32Converters();
			RegisterUInt64Converters();
			RegisterFloat32Converters();
			RegisterFloat64Converters();
			RegisterBooleanConverters();
			RegisterCharConverters();
			RegisterStringConverters();
			RegisterObjectConverters();
			s_GlobalConverters.Register(typeof(string), typeof(Guid), (TypeConverter<string, Guid>)delegate(ref string g)
			{
				return new Guid(g);
			});
		}

		private static void RegisterInt8Converters()
		{
			s_GlobalConverters.Register(typeof(sbyte), typeof(char), (TypeConverter<sbyte, char>)delegate(ref sbyte v)
			{
				return (char)v;
			});
			s_GlobalConverters.Register(typeof(sbyte), typeof(bool), (TypeConverter<sbyte, bool>)delegate(ref sbyte v)
			{
				return v != 0;
			});
			s_GlobalConverters.Register(typeof(sbyte), typeof(short), (TypeConverter<sbyte, short>)delegate(ref sbyte v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(sbyte), typeof(int), (TypeConverter<sbyte, int>)delegate(ref sbyte v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(sbyte), typeof(long), (TypeConverter<sbyte, long>)delegate(ref sbyte v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(sbyte), typeof(byte), (TypeConverter<sbyte, byte>)delegate(ref sbyte v)
			{
				return (byte)v;
			});
			s_GlobalConverters.Register(typeof(sbyte), typeof(ushort), (TypeConverter<sbyte, ushort>)delegate(ref sbyte v)
			{
				return (ushort)v;
			});
			s_GlobalConverters.Register(typeof(sbyte), typeof(uint), (TypeConverter<sbyte, uint>)delegate(ref sbyte v)
			{
				return (uint)v;
			});
			s_GlobalConverters.Register(typeof(sbyte), typeof(ulong), (TypeConverter<sbyte, ulong>)delegate(ref sbyte v)
			{
				return (ulong)v;
			});
			s_GlobalConverters.Register(typeof(sbyte), typeof(float), (TypeConverter<sbyte, float>)delegate(ref sbyte v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(sbyte), typeof(double), (TypeConverter<sbyte, double>)delegate(ref sbyte v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(sbyte), typeof(object), (TypeConverter<sbyte, object>)delegate(ref sbyte v)
			{
				return v;
			});
		}

		private static void RegisterInt16Converters()
		{
			s_GlobalConverters.Register(typeof(short), typeof(sbyte), (TypeConverter<short, sbyte>)delegate(ref short v)
			{
				return (sbyte)v;
			});
			s_GlobalConverters.Register(typeof(short), typeof(char), (TypeConverter<short, char>)delegate(ref short v)
			{
				return (char)v;
			});
			s_GlobalConverters.Register(typeof(short), typeof(bool), (TypeConverter<short, bool>)delegate(ref short v)
			{
				return v != 0;
			});
			s_GlobalConverters.Register(typeof(short), typeof(int), (TypeConverter<short, int>)delegate(ref short v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(short), typeof(long), (TypeConverter<short, long>)delegate(ref short v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(short), typeof(byte), (TypeConverter<short, byte>)delegate(ref short v)
			{
				return (byte)v;
			});
			s_GlobalConverters.Register(typeof(short), typeof(ushort), (TypeConverter<short, ushort>)delegate(ref short v)
			{
				return (ushort)v;
			});
			s_GlobalConverters.Register(typeof(short), typeof(uint), (TypeConverter<short, uint>)delegate(ref short v)
			{
				return (uint)v;
			});
			s_GlobalConverters.Register(typeof(short), typeof(ulong), (TypeConverter<short, ulong>)delegate(ref short v)
			{
				return (ulong)v;
			});
			s_GlobalConverters.Register(typeof(short), typeof(float), (TypeConverter<short, float>)delegate(ref short v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(short), typeof(double), (TypeConverter<short, double>)delegate(ref short v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(short), typeof(object), (TypeConverter<short, object>)delegate(ref short v)
			{
				return v;
			});
		}

		private static void RegisterInt32Converters()
		{
			s_GlobalConverters.Register(typeof(int), typeof(sbyte), (TypeConverter<int, sbyte>)delegate(ref int v)
			{
				return (sbyte)v;
			});
			s_GlobalConverters.Register(typeof(int), typeof(char), (TypeConverter<int, char>)delegate(ref int v)
			{
				return (char)v;
			});
			s_GlobalConverters.Register(typeof(int), typeof(bool), (TypeConverter<int, bool>)delegate(ref int v)
			{
				return v != 0;
			});
			s_GlobalConverters.Register(typeof(int), typeof(short), (TypeConverter<int, short>)delegate(ref int v)
			{
				return (short)v;
			});
			s_GlobalConverters.Register(typeof(int), typeof(long), (TypeConverter<int, long>)delegate(ref int v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(int), typeof(byte), (TypeConverter<int, byte>)delegate(ref int v)
			{
				return (byte)v;
			});
			s_GlobalConverters.Register(typeof(int), typeof(ushort), (TypeConverter<int, ushort>)delegate(ref int v)
			{
				return (ushort)v;
			});
			s_GlobalConverters.Register(typeof(int), typeof(uint), (TypeConverter<int, uint>)delegate(ref int v)
			{
				return (uint)v;
			});
			s_GlobalConverters.Register(typeof(int), typeof(ulong), (TypeConverter<int, ulong>)delegate(ref int v)
			{
				return (ulong)v;
			});
			s_GlobalConverters.Register(typeof(int), typeof(float), (TypeConverter<int, float>)delegate(ref int v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(int), typeof(double), (TypeConverter<int, double>)delegate(ref int v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(int), typeof(object), (TypeConverter<int, object>)delegate(ref int v)
			{
				return v;
			});
		}

		private static void RegisterInt64Converters()
		{
			s_GlobalConverters.Register(typeof(long), typeof(sbyte), (TypeConverter<long, sbyte>)delegate(ref long v)
			{
				return (sbyte)v;
			});
			s_GlobalConverters.Register(typeof(long), typeof(char), (TypeConverter<long, char>)delegate(ref long v)
			{
				return (char)v;
			});
			s_GlobalConverters.Register(typeof(long), typeof(bool), (TypeConverter<long, bool>)delegate(ref long v)
			{
				return v != 0;
			});
			s_GlobalConverters.Register(typeof(long), typeof(short), (TypeConverter<long, short>)delegate(ref long v)
			{
				return (short)v;
			});
			s_GlobalConverters.Register(typeof(long), typeof(int), (TypeConverter<long, int>)delegate(ref long v)
			{
				return (int)v;
			});
			s_GlobalConverters.Register(typeof(long), typeof(byte), (TypeConverter<long, byte>)delegate(ref long v)
			{
				return (byte)v;
			});
			s_GlobalConverters.Register(typeof(long), typeof(ushort), (TypeConverter<long, ushort>)delegate(ref long v)
			{
				return (ushort)v;
			});
			s_GlobalConverters.Register(typeof(long), typeof(uint), (TypeConverter<long, uint>)delegate(ref long v)
			{
				return (uint)v;
			});
			s_GlobalConverters.Register(typeof(long), typeof(ulong), (TypeConverter<long, ulong>)delegate(ref long v)
			{
				return (ulong)v;
			});
			s_GlobalConverters.Register(typeof(long), typeof(float), (TypeConverter<long, float>)delegate(ref long v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(long), typeof(double), (TypeConverter<long, double>)delegate(ref long v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(long), typeof(object), (TypeConverter<long, object>)delegate(ref long v)
			{
				return v;
			});
		}

		private static void RegisterUInt8Converters()
		{
			s_GlobalConverters.Register(typeof(byte), typeof(sbyte), (TypeConverter<byte, sbyte>)delegate(ref byte v)
			{
				return (sbyte)v;
			});
			s_GlobalConverters.Register(typeof(byte), typeof(char), (TypeConverter<byte, char>)delegate(ref byte v)
			{
				return (char)v;
			});
			s_GlobalConverters.Register(typeof(byte), typeof(bool), (TypeConverter<byte, bool>)delegate(ref byte v)
			{
				return v != 0;
			});
			s_GlobalConverters.Register(typeof(byte), typeof(short), (TypeConverter<byte, short>)delegate(ref byte v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(byte), typeof(int), (TypeConverter<byte, int>)delegate(ref byte v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(byte), typeof(long), (TypeConverter<byte, long>)delegate(ref byte v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(byte), typeof(ushort), (TypeConverter<byte, ushort>)delegate(ref byte v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(byte), typeof(uint), (TypeConverter<byte, uint>)delegate(ref byte v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(byte), typeof(ulong), (TypeConverter<byte, ulong>)delegate(ref byte v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(byte), typeof(float), (TypeConverter<byte, float>)delegate(ref byte v)
			{
				return (int)v;
			});
			s_GlobalConverters.Register(typeof(byte), typeof(double), (TypeConverter<byte, double>)delegate(ref byte v)
			{
				return (int)v;
			});
			s_GlobalConverters.Register(typeof(byte), typeof(object), (TypeConverter<byte, object>)delegate(ref byte v)
			{
				return v;
			});
		}

		private static void RegisterUInt16Converters()
		{
			s_GlobalConverters.Register(typeof(ushort), typeof(sbyte), (TypeConverter<ushort, sbyte>)delegate(ref ushort v)
			{
				return (sbyte)v;
			});
			s_GlobalConverters.Register(typeof(ushort), typeof(char), (TypeConverter<ushort, char>)delegate(ref ushort v)
			{
				return (char)v;
			});
			s_GlobalConverters.Register(typeof(ushort), typeof(bool), (TypeConverter<ushort, bool>)delegate(ref ushort v)
			{
				return v != 0;
			});
			s_GlobalConverters.Register(typeof(ushort), typeof(short), (TypeConverter<ushort, short>)delegate(ref ushort v)
			{
				return (short)v;
			});
			s_GlobalConverters.Register(typeof(ushort), typeof(int), (TypeConverter<ushort, int>)delegate(ref ushort v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(ushort), typeof(long), (TypeConverter<ushort, long>)delegate(ref ushort v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(ushort), typeof(byte), (TypeConverter<ushort, byte>)delegate(ref ushort v)
			{
				return (byte)v;
			});
			s_GlobalConverters.Register(typeof(ushort), typeof(uint), (TypeConverter<ushort, uint>)delegate(ref ushort v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(ushort), typeof(ulong), (TypeConverter<ushort, ulong>)delegate(ref ushort v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(ushort), typeof(float), (TypeConverter<ushort, float>)delegate(ref ushort v)
			{
				return (int)v;
			});
			s_GlobalConverters.Register(typeof(ushort), typeof(double), (TypeConverter<ushort, double>)delegate(ref ushort v)
			{
				return (int)v;
			});
			s_GlobalConverters.Register(typeof(ushort), typeof(object), (TypeConverter<ushort, object>)delegate(ref ushort v)
			{
				return v;
			});
		}

		private static void RegisterUInt32Converters()
		{
			s_GlobalConverters.Register(typeof(uint), typeof(sbyte), (TypeConverter<uint, sbyte>)delegate(ref uint v)
			{
				return (sbyte)v;
			});
			s_GlobalConverters.Register(typeof(uint), typeof(char), (TypeConverter<uint, char>)delegate(ref uint v)
			{
				return (char)v;
			});
			s_GlobalConverters.Register(typeof(uint), typeof(bool), (TypeConverter<uint, bool>)delegate(ref uint v)
			{
				return v != 0;
			});
			s_GlobalConverters.Register(typeof(uint), typeof(short), (TypeConverter<uint, short>)delegate(ref uint v)
			{
				return (short)v;
			});
			s_GlobalConverters.Register(typeof(uint), typeof(int), (TypeConverter<uint, int>)delegate(ref uint v)
			{
				return (int)v;
			});
			s_GlobalConverters.Register(typeof(uint), typeof(long), (TypeConverter<uint, long>)delegate(ref uint v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(uint), typeof(byte), (TypeConverter<uint, byte>)delegate(ref uint v)
			{
				return (byte)v;
			});
			s_GlobalConverters.Register(typeof(uint), typeof(ushort), (TypeConverter<uint, ushort>)delegate(ref uint v)
			{
				return (ushort)v;
			});
			s_GlobalConverters.Register(typeof(uint), typeof(ulong), (TypeConverter<uint, ulong>)delegate(ref uint v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(uint), typeof(float), (TypeConverter<uint, float>)delegate(ref uint v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(uint), typeof(double), (TypeConverter<uint, double>)delegate(ref uint v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(uint), typeof(object), (TypeConverter<uint, object>)delegate(ref uint v)
			{
				return v;
			});
		}

		private static void RegisterUInt64Converters()
		{
			s_GlobalConverters.Register(typeof(ulong), typeof(sbyte), (TypeConverter<ulong, sbyte>)delegate(ref ulong v)
			{
				return (sbyte)v;
			});
			s_GlobalConverters.Register(typeof(ulong), typeof(char), (TypeConverter<ulong, char>)delegate(ref ulong v)
			{
				return (char)v;
			});
			s_GlobalConverters.Register(typeof(ulong), typeof(bool), (TypeConverter<ulong, bool>)delegate(ref ulong v)
			{
				return v != 0;
			});
			s_GlobalConverters.Register(typeof(ulong), typeof(short), (TypeConverter<ulong, short>)delegate(ref ulong v)
			{
				return (short)v;
			});
			s_GlobalConverters.Register(typeof(ulong), typeof(int), (TypeConverter<ulong, int>)delegate(ref ulong v)
			{
				return (int)v;
			});
			s_GlobalConverters.Register(typeof(ulong), typeof(long), (TypeConverter<ulong, long>)delegate(ref ulong v)
			{
				return (long)v;
			});
			s_GlobalConverters.Register(typeof(ulong), typeof(byte), (TypeConverter<ulong, byte>)delegate(ref ulong v)
			{
				return (byte)v;
			});
			s_GlobalConverters.Register(typeof(ulong), typeof(ushort), (TypeConverter<ulong, ushort>)delegate(ref ulong v)
			{
				return (ushort)v;
			});
			s_GlobalConverters.Register(typeof(ulong), typeof(uint), (TypeConverter<ulong, uint>)delegate(ref ulong v)
			{
				return (uint)v;
			});
			s_GlobalConverters.Register(typeof(ulong), typeof(float), (TypeConverter<ulong, float>)delegate(ref ulong v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(ulong), typeof(double), (TypeConverter<ulong, double>)delegate(ref ulong v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(ulong), typeof(object), (TypeConverter<ulong, object>)delegate(ref ulong v)
			{
				return v;
			});
		}

		private static void RegisterFloat32Converters()
		{
			s_GlobalConverters.Register(typeof(float), typeof(sbyte), (TypeConverter<float, sbyte>)delegate(ref float v)
			{
				return (sbyte)v;
			});
			s_GlobalConverters.Register(typeof(float), typeof(char), (TypeConverter<float, char>)delegate(ref float v)
			{
				return (char)v;
			});
			s_GlobalConverters.Register(typeof(float), typeof(bool), (TypeConverter<float, bool>)delegate(ref float v)
			{
				return Math.Abs(v) > float.Epsilon;
			});
			s_GlobalConverters.Register(typeof(float), typeof(short), (TypeConverter<float, short>)delegate(ref float v)
			{
				return (short)v;
			});
			s_GlobalConverters.Register(typeof(float), typeof(int), (TypeConverter<float, int>)delegate(ref float v)
			{
				return (int)v;
			});
			s_GlobalConverters.Register(typeof(float), typeof(long), (TypeConverter<float, long>)delegate(ref float v)
			{
				return (long)v;
			});
			s_GlobalConverters.Register(typeof(float), typeof(byte), (TypeConverter<float, byte>)delegate(ref float v)
			{
				return (byte)v;
			});
			s_GlobalConverters.Register(typeof(float), typeof(ushort), (TypeConverter<float, ushort>)delegate(ref float v)
			{
				return (ushort)v;
			});
			s_GlobalConverters.Register(typeof(float), typeof(uint), (TypeConverter<float, uint>)delegate(ref float v)
			{
				return (uint)v;
			});
			s_GlobalConverters.Register(typeof(float), typeof(ulong), (TypeConverter<float, ulong>)delegate(ref float v)
			{
				return (ulong)v;
			});
			s_GlobalConverters.Register(typeof(float), typeof(double), (TypeConverter<float, double>)delegate(ref float v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(float), typeof(object), (TypeConverter<float, object>)delegate(ref float v)
			{
				return v;
			});
		}

		private static void RegisterFloat64Converters()
		{
			s_GlobalConverters.Register(typeof(double), typeof(sbyte), (TypeConverter<double, sbyte>)delegate(ref double v)
			{
				return (sbyte)v;
			});
			s_GlobalConverters.Register(typeof(double), typeof(char), (TypeConverter<double, char>)delegate(ref double v)
			{
				return (char)v;
			});
			s_GlobalConverters.Register(typeof(double), typeof(bool), (TypeConverter<double, bool>)delegate(ref double v)
			{
				return Math.Abs(v) > double.Epsilon;
			});
			s_GlobalConverters.Register(typeof(double), typeof(short), (TypeConverter<double, short>)delegate(ref double v)
			{
				return (short)v;
			});
			s_GlobalConverters.Register(typeof(double), typeof(int), (TypeConverter<double, int>)delegate(ref double v)
			{
				return (int)v;
			});
			s_GlobalConverters.Register(typeof(double), typeof(long), (TypeConverter<double, long>)delegate(ref double v)
			{
				return (long)v;
			});
			s_GlobalConverters.Register(typeof(double), typeof(byte), (TypeConverter<double, byte>)delegate(ref double v)
			{
				return (byte)v;
			});
			s_GlobalConverters.Register(typeof(double), typeof(ushort), (TypeConverter<double, ushort>)delegate(ref double v)
			{
				return (ushort)v;
			});
			s_GlobalConverters.Register(typeof(double), typeof(uint), (TypeConverter<double, uint>)delegate(ref double v)
			{
				return (uint)v;
			});
			s_GlobalConverters.Register(typeof(double), typeof(ulong), (TypeConverter<double, ulong>)delegate(ref double v)
			{
				return (ulong)v;
			});
			s_GlobalConverters.Register(typeof(double), typeof(float), (TypeConverter<double, float>)delegate(ref double v)
			{
				return (float)v;
			});
			s_GlobalConverters.Register(typeof(double), typeof(object), (TypeConverter<double, object>)delegate(ref double v)
			{
				return v;
			});
		}

		private static void RegisterBooleanConverters()
		{
			s_GlobalConverters.Register(typeof(bool), typeof(char), (TypeConverter<bool, char>)delegate(ref bool v)
			{
				return v ? '\u0001' : '\0';
			});
			s_GlobalConverters.Register(typeof(bool), typeof(sbyte), (TypeConverter<bool, sbyte>)delegate(ref bool v)
			{
				return (sbyte)(v ? 1 : 0);
			});
			s_GlobalConverters.Register(typeof(bool), typeof(short), (TypeConverter<bool, short>)delegate(ref bool v)
			{
				return (short)(v ? 1 : 0);
			});
			s_GlobalConverters.Register(typeof(bool), typeof(int), (TypeConverter<bool, int>)delegate(ref bool v)
			{
				return v ? 1 : 0;
			});
			s_GlobalConverters.Register(typeof(bool), typeof(long), (TypeConverter<bool, long>)delegate(ref bool v)
			{
				return v ? 1 : 0;
			});
			s_GlobalConverters.Register(typeof(bool), typeof(byte), (TypeConverter<bool, byte>)delegate(ref bool v)
			{
				return (byte)(v ? 1 : 0);
			});
			s_GlobalConverters.Register(typeof(bool), typeof(ushort), (TypeConverter<bool, ushort>)delegate(ref bool v)
			{
				return (ushort)(v ? 1 : 0);
			});
			s_GlobalConverters.Register(typeof(bool), typeof(uint), (TypeConverter<bool, uint>)delegate(ref bool v)
			{
				return v ? 1u : 0u;
			});
			s_GlobalConverters.Register(typeof(bool), typeof(ulong), (TypeConverter<bool, ulong>)delegate(ref bool v)
			{
				return (ulong)(v ? 1 : 0);
			});
			s_GlobalConverters.Register(typeof(bool), typeof(float), (TypeConverter<bool, float>)delegate(ref bool v)
			{
				return v ? 1f : 0f;
			});
			s_GlobalConverters.Register(typeof(bool), typeof(double), (TypeConverter<bool, double>)delegate(ref bool v)
			{
				return v ? 1.0 : 0.0;
			});
			s_GlobalConverters.Register(typeof(bool), typeof(object), (TypeConverter<bool, object>)delegate(ref bool v)
			{
				return v;
			});
		}

		private static void RegisterCharConverters()
		{
			s_GlobalConverters.Register(typeof(string), typeof(char), (TypeConverter<string, char>)delegate(ref string v)
			{
				if (v.Length != 1)
				{
					throw new Exception("Not a valid char");
				}
				return v[0];
			});
			s_GlobalConverters.Register(typeof(char), typeof(bool), (TypeConverter<char, bool>)delegate(ref char v)
			{
				return v != '\0';
			});
			s_GlobalConverters.Register(typeof(char), typeof(sbyte), (TypeConverter<char, sbyte>)delegate(ref char v)
			{
				return (sbyte)v;
			});
			s_GlobalConverters.Register(typeof(char), typeof(short), (TypeConverter<char, short>)delegate(ref char v)
			{
				return (short)v;
			});
			s_GlobalConverters.Register(typeof(char), typeof(int), (TypeConverter<char, int>)delegate(ref char v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(char), typeof(long), (TypeConverter<char, long>)delegate(ref char v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(char), typeof(byte), (TypeConverter<char, byte>)delegate(ref char v)
			{
				return (byte)v;
			});
			s_GlobalConverters.Register(typeof(char), typeof(ushort), (TypeConverter<char, ushort>)delegate(ref char v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(char), typeof(uint), (TypeConverter<char, uint>)delegate(ref char v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(char), typeof(ulong), (TypeConverter<char, ulong>)delegate(ref char v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(char), typeof(float), (TypeConverter<char, float>)delegate(ref char v)
			{
				return (int)v;
			});
			s_GlobalConverters.Register(typeof(char), typeof(double), (TypeConverter<char, double>)delegate(ref char v)
			{
				return (int)v;
			});
			s_GlobalConverters.Register(typeof(char), typeof(object), (TypeConverter<char, object>)delegate(ref char v)
			{
				return v;
			});
			s_GlobalConverters.Register(typeof(char), typeof(string), (TypeConverter<char, string>)delegate(ref char v)
			{
				return v.ToString();
			});
		}

		private static void RegisterStringConverters()
		{
			s_GlobalConverters.Register(typeof(string), typeof(char), (TypeConverter<string, char>)delegate(ref string v)
			{
				return (!string.IsNullOrEmpty(v)) ? v[0] : '\0';
			});
			s_GlobalConverters.Register(typeof(char), typeof(string), (TypeConverter<char, string>)delegate(ref char v)
			{
				return v.ToString();
			});
			s_GlobalConverters.Register(typeof(string), typeof(bool), (TypeConverter<string, bool>)delegate(ref string v)
			{
				bool result;
				double result2;
				return bool.TryParse(v, out result) ? result : (double.TryParse(v, out result2) && Convert<double, bool>(ref result2));
			});
			s_GlobalConverters.Register(typeof(bool), typeof(string), (TypeConverter<bool, string>)delegate(ref bool v)
			{
				return v.ToString();
			});
			s_GlobalConverters.Register(typeof(string), typeof(sbyte), (TypeConverter<string, sbyte>)delegate(ref string v)
			{
				sbyte result;
				double result2;
				return (sbyte)(sbyte.TryParse(v, out result) ? result : (double.TryParse(v, out result2) ? Convert<double, sbyte>(ref result2) : 0));
			});
			s_GlobalConverters.Register(typeof(sbyte), typeof(string), (TypeConverter<sbyte, string>)delegate(ref sbyte v)
			{
				return v.ToString();
			});
			s_GlobalConverters.Register(typeof(string), typeof(short), (TypeConverter<string, short>)delegate(ref string v)
			{
				short result;
				double result2;
				return (short)(short.TryParse(v, out result) ? result : (double.TryParse(v, out result2) ? Convert<double, short>(ref result2) : 0));
			});
			s_GlobalConverters.Register(typeof(short), typeof(string), (TypeConverter<short, string>)delegate(ref short v)
			{
				return v.ToString();
			});
			s_GlobalConverters.Register(typeof(string), typeof(int), (TypeConverter<string, int>)delegate(ref string v)
			{
				int result;
				double result2;
				return int.TryParse(v, out result) ? result : (double.TryParse(v, out result2) ? Convert<double, int>(ref result2) : 0);
			});
			s_GlobalConverters.Register(typeof(int), typeof(string), (TypeConverter<int, string>)delegate(ref int v)
			{
				return v.ToString();
			});
			s_GlobalConverters.Register(typeof(string), typeof(long), (TypeConverter<string, long>)delegate(ref string v)
			{
				long result;
				double result2;
				return long.TryParse(v, out result) ? result : (double.TryParse(v, out result2) ? Convert<double, long>(ref result2) : 0);
			});
			s_GlobalConverters.Register(typeof(long), typeof(string), (TypeConverter<long, string>)delegate(ref long v)
			{
				return v.ToString();
			});
			s_GlobalConverters.Register(typeof(string), typeof(byte), (TypeConverter<string, byte>)delegate(ref string v)
			{
				byte result;
				double result2;
				return (byte)(byte.TryParse(v, out result) ? result : (double.TryParse(v, out result2) ? Convert<double, byte>(ref result2) : 0));
			});
			s_GlobalConverters.Register(typeof(byte), typeof(string), (TypeConverter<byte, string>)delegate(ref byte v)
			{
				return v.ToString();
			});
			s_GlobalConverters.Register(typeof(string), typeof(ushort), (TypeConverter<string, ushort>)delegate(ref string v)
			{
				ushort result;
				double result2;
				return (ushort)(ushort.TryParse(v, out result) ? result : (double.TryParse(v, out result2) ? Convert<double, ushort>(ref result2) : 0));
			});
			s_GlobalConverters.Register(typeof(ushort), typeof(string), (TypeConverter<ushort, string>)delegate(ref ushort v)
			{
				return v.ToString();
			});
			s_GlobalConverters.Register(typeof(string), typeof(uint), (TypeConverter<string, uint>)delegate(ref string v)
			{
				uint result;
				double result2;
				return uint.TryParse(v, out result) ? result : (double.TryParse(v, out result2) ? Convert<double, uint>(ref result2) : 0u);
			});
			s_GlobalConverters.Register(typeof(uint), typeof(string), (TypeConverter<uint, string>)delegate(ref uint v)
			{
				return v.ToString();
			});
			s_GlobalConverters.Register(typeof(string), typeof(ulong), (TypeConverter<string, ulong>)delegate(ref string v)
			{
				ulong result;
				double result2;
				return ulong.TryParse(v, out result) ? result : (double.TryParse(v, out result2) ? Convert<double, ulong>(ref result2) : 0);
			});
			s_GlobalConverters.Register(typeof(ulong), typeof(string), (TypeConverter<ulong, string>)delegate(ref ulong v)
			{
				return v.ToString();
			});
			s_GlobalConverters.Register(typeof(string), typeof(float), (TypeConverter<string, float>)delegate(ref string v)
			{
				float result;
				double result2;
				return float.TryParse(v, out result) ? result : (double.TryParse(v, out result2) ? Convert<double, float>(ref result2) : 0f);
			});
			s_GlobalConverters.Register(typeof(float), typeof(string), (TypeConverter<float, string>)delegate(ref float v)
			{
				return v.ToString(CultureInfo.InvariantCulture);
			});
			s_GlobalConverters.Register(typeof(string), typeof(double), (TypeConverter<string, double>)delegate(ref string v)
			{
				double.TryParse(v, out var result);
				return result;
			});
			s_GlobalConverters.Register(typeof(double), typeof(string), (TypeConverter<double, string>)delegate(ref double v)
			{
				return v.ToString(CultureInfo.InvariantCulture);
			});
		}

		private static void RegisterObjectConverters()
		{
			s_GlobalConverters.Register(typeof(object), typeof(char), (TypeConverter<object, char>)delegate(ref object v)
			{
				return (v is char c) ? c : '\0';
			});
			s_GlobalConverters.Register(typeof(object), typeof(bool), (TypeConverter<object, bool>)delegate(ref object v)
			{
				return v is bool flag && flag;
			});
			s_GlobalConverters.Register(typeof(object), typeof(sbyte), (TypeConverter<object, sbyte>)delegate(ref object v)
			{
				return (sbyte)((v is sbyte b) ? b : 0);
			});
			s_GlobalConverters.Register(typeof(object), typeof(short), (TypeConverter<object, short>)delegate(ref object v)
			{
				return (short)((v is short num) ? num : 0);
			});
			s_GlobalConverters.Register(typeof(object), typeof(int), (TypeConverter<object, int>)delegate(ref object v)
			{
				return (v is int num) ? num : 0;
			});
			s_GlobalConverters.Register(typeof(object), typeof(long), (TypeConverter<object, long>)delegate(ref object v)
			{
				return (v is long num) ? num : 0;
			});
			s_GlobalConverters.Register(typeof(object), typeof(byte), (TypeConverter<object, byte>)delegate(ref object v)
			{
				return (byte)((v is byte b) ? b : 0);
			});
			s_GlobalConverters.Register(typeof(object), typeof(ushort), (TypeConverter<object, ushort>)delegate(ref object v)
			{
				return (ushort)((v is ushort num) ? num : 0);
			});
			s_GlobalConverters.Register(typeof(object), typeof(uint), (TypeConverter<object, uint>)delegate(ref object v)
			{
				return (v is uint num) ? num : 0u;
			});
			s_GlobalConverters.Register(typeof(object), typeof(ulong), (TypeConverter<object, ulong>)delegate(ref object v)
			{
				return (v is ulong num) ? num : 0;
			});
			s_GlobalConverters.Register(typeof(object), typeof(float), (TypeConverter<object, float>)delegate(ref object v)
			{
				return (v is float num) ? num : 0f;
			});
			s_GlobalConverters.Register(typeof(object), typeof(double), (TypeConverter<object, double>)delegate(ref object v)
			{
				return (v is double num) ? num : 0.0;
			});
		}
	}

	private static readonly ConversionRegistry s_GlobalConverters;

	static TypeConversion()
	{
		s_GlobalConverters = ConversionRegistry.Create();
		PrimitiveConverters.Register();
	}

	public static void Register<TSource, TDestination>(TypeConverter<TSource, TDestination> converter)
	{
		s_GlobalConverters.Register(typeof(TSource), typeof(TDestination), converter);
	}

	public static TDestination Convert<TSource, TDestination>(ref TSource value)
	{
		if (!TryConvert<TSource, TDestination>(ref value, out var destination))
		{
			throw new InvalidOperationException($"TypeConversion no converter has been registered for SrcType=[{typeof(TSource)}] to DstType=[{typeof(TDestination)}]");
		}
		return destination;
	}

	public static bool TryConvert<TSource, TDestination>(ref TSource source, out TDestination destination)
	{
		if (s_GlobalConverters.TryGetConverter(typeof(TSource), typeof(TDestination), out var converter))
		{
			destination = ((TypeConverter<TSource, TDestination>)converter)(ref source);
			return true;
		}
		if (typeof(TSource).IsValueType && typeof(TSource) == typeof(TDestination))
		{
			destination = UnsafeUtility.As<TSource, TDestination>(ref source);
			return true;
		}
		if (TypeTraits<TDestination>.IsNullable)
		{
			if (TypeTraits<TSource>.IsNullable && Nullable.GetUnderlyingType(typeof(TDestination)) != Nullable.GetUnderlyingType(typeof(TSource)))
			{
				destination = default(TDestination);
				return false;
			}
			Type underlyingType = Nullable.GetUnderlyingType(typeof(TDestination));
			if (underlyingType.IsEnum)
			{
				Type underlyingType2 = Enum.GetUnderlyingType(underlyingType);
				object value = System.Convert.ChangeType(source, underlyingType2);
				destination = (TDestination)Enum.ToObject(underlyingType, value);
				return true;
			}
			if (source == null)
			{
				destination = default(TDestination);
				return true;
			}
			destination = (TDestination)System.Convert.ChangeType(source, underlyingType);
			return true;
		}
		if (TypeTraits<TSource>.IsNullable && typeof(TDestination) == Nullable.GetUnderlyingType(typeof(TSource)))
		{
			if (source == null)
			{
				destination = default(TDestination);
				return false;
			}
			destination = (TDestination)(object)source;
			return true;
		}
		if (TypeTraits<TDestination>.IsUnityObject && TryConvertToUnityEngineObject<TSource, TDestination>(source, out destination))
		{
			return true;
		}
		if (TypeTraits<TDestination>.IsEnum)
		{
			if (typeof(TSource) == typeof(string))
			{
				try
				{
					destination = (TDestination)Enum.Parse(typeof(TDestination), (string)(object)source);
				}
				catch (ArgumentException)
				{
					destination = default(TDestination);
					return false;
				}
				return true;
			}
			if (IsNumericType(typeof(TSource)))
			{
				destination = UnsafeUtility.As<TSource, TDestination>(ref source);
				return true;
			}
		}
		TSource val = source;
		if (val is TDestination val2)
		{
			destination = val2;
			return true;
		}
		if (typeof(TDestination).IsAssignableFrom(typeof(TSource)))
		{
			destination = (TDestination)(object)source;
			return true;
		}
		destination = default(TDestination);
		return false;
	}

	private static bool TryConvertToUnityEngineObject<TSource, TDestination>(TSource source, out TDestination destination)
	{
		if (!typeof(UnityEngine.Object).IsAssignableFrom(typeof(TDestination)))
		{
			destination = default(TDestination);
			return false;
		}
		if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(TSource)) || source is UnityEngine.Object)
		{
			if (source == null)
			{
				destination = default(TDestination);
				return true;
			}
			if (typeof(TDestination) == typeof(UnityEngine.Object))
			{
				destination = (TDestination)(object)source;
				return true;
			}
		}
		if (s_GlobalConverters.TryGetConverter(typeof(TSource), typeof(UnityEngine.Object), out var converter))
		{
			UnityEngine.Object obj = ((TypeConverter<TSource, UnityEngine.Object>)converter)(ref source);
			destination = (TDestination)(object)obj;
			return obj;
		}
		destination = default(TDestination);
		return false;
	}

	private static bool IsNumericType(Type t)
	{
		TypeCode typeCode = Type.GetTypeCode(t);
		TypeCode typeCode2 = typeCode;
		if ((uint)(typeCode2 - 5) <= 10u)
		{
			return true;
		}
		return false;
	}
}
