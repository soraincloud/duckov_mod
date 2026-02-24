using System;
using System.IO;

namespace Unity.VisualScripting.Antlr3.Runtime;

public class ANTLRReaderStream : ANTLRStringStream
{
	public static readonly int READ_BUFFER_SIZE = 1024;

	public static readonly int INITIAL_BUFFER_SIZE = 1024;

	protected ANTLRReaderStream()
	{
	}

	public ANTLRReaderStream(TextReader reader)
		: this(reader, INITIAL_BUFFER_SIZE, READ_BUFFER_SIZE)
	{
	}

	public ANTLRReaderStream(TextReader reader, int size)
		: this(reader, size, READ_BUFFER_SIZE)
	{
	}

	public ANTLRReaderStream(TextReader reader, int size, int readChunkSize)
	{
		Load(reader, size, readChunkSize);
	}

	public virtual void Load(TextReader reader, int size, int readChunkSize)
	{
		if (reader == null)
		{
			return;
		}
		if (size <= 0)
		{
			size = INITIAL_BUFFER_SIZE;
		}
		if (readChunkSize <= 0)
		{
			readChunkSize = READ_BUFFER_SIZE;
		}
		try
		{
			data = new char[size];
			int num = 0;
			int num2 = 0;
			do
			{
				if (num2 + readChunkSize > data.Length)
				{
					char[] destinationArray = new char[data.Length * 2];
					Array.Copy(data, 0, destinationArray, 0, data.Length);
					data = destinationArray;
				}
				num = reader.Read(data, num2, readChunkSize);
				num2 += num;
			}
			while (num != 0);
			n = num2;
		}
		finally
		{
			reader.Close();
		}
	}
}
