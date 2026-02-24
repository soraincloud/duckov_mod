using System;

namespace Unity.VisualScripting.Antlr3.Runtime;

public interface IIntStream
{
	int Count { get; }

	string SourceName { get; }

	void Consume();

	int LA(int i);

	int Mark();

	int Index();

	void Rewind(int marker);

	void Rewind();

	void Release(int marker);

	void Seek(int index);

	[Obsolete("Please use property Count instead.")]
	int Size();
}
