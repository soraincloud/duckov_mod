using System.IO;
using System.Text;

namespace Unity.VisualScripting.Antlr3.Runtime;

public class ANTLRInputStream : ANTLRReaderStream
{
	protected ANTLRInputStream()
	{
	}

	public ANTLRInputStream(Stream istream)
		: this(istream, null)
	{
	}

	public ANTLRInputStream(Stream istream, Encoding encoding)
		: this(istream, ANTLRReaderStream.INITIAL_BUFFER_SIZE, encoding)
	{
	}

	public ANTLRInputStream(Stream istream, int size)
		: this(istream, size, null)
	{
	}

	public ANTLRInputStream(Stream istream, int size, Encoding encoding)
		: this(istream, size, ANTLRReaderStream.READ_BUFFER_SIZE, encoding)
	{
	}

	public ANTLRInputStream(Stream istream, int size, int readBufferSize, Encoding encoding)
	{
		Load((encoding == null) ? new StreamReader(istream) : new StreamReader(istream, encoding), size, readBufferSize);
	}
}
