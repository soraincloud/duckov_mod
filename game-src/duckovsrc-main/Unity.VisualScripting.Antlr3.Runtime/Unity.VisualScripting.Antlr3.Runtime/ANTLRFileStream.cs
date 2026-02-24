using System.IO;
using System.Text;

namespace Unity.VisualScripting.Antlr3.Runtime;

public class ANTLRFileStream : ANTLRStringStream
{
	protected string fileName;

	public override string SourceName => fileName;

	protected ANTLRFileStream()
	{
	}

	public ANTLRFileStream(string fileName)
		: this(fileName, Encoding.Default)
	{
	}

	public ANTLRFileStream(string fileName, Encoding encoding)
	{
		this.fileName = fileName;
		Load(fileName, encoding);
	}

	public virtual void Load(string fileName, Encoding encoding)
	{
		if (fileName == null)
		{
			return;
		}
		StreamReader streamReader = null;
		try
		{
			FileInfo file = new FileInfo(fileName);
			int num = (int)GetFileLength(file);
			data = new char[num];
			streamReader = ((encoding == null) ? new StreamReader(fileName, Encoding.Default) : new StreamReader(fileName, encoding));
			n = streamReader.Read(data, 0, data.Length);
		}
		finally
		{
			streamReader?.Close();
		}
	}

	private long GetFileLength(FileInfo file)
	{
		if (file.Exists)
		{
			return file.Length;
		}
		return 0L;
	}
}
