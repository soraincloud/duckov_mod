using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Windows.Speech;

public abstract class PhraseRecognizer : IDisposable
{
	public delegate void PhraseRecognizedDelegate(PhraseRecognizedEventArgs args);

	protected IntPtr m_Recognizer;

	public bool IsRunning => m_Recognizer != IntPtr.Zero && IsRunning_Internal(m_Recognizer);

	public event PhraseRecognizedDelegate OnPhraseRecognized;

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeHeader("PlatformDependent/Win/Bindings/SpeechBindings.h")]
	[NativeThrows]
	protected static extern IntPtr CreateFromKeywords(object self, [Unmarshalled] string[] keywords, ConfidenceLevel minimumConfidence);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeHeader("PlatformDependent/Win/Bindings/SpeechBindings.h")]
	[NativeThrows]
	protected static extern IntPtr CreateFromGrammarFile(object self, string grammarFilePath, ConfidenceLevel minimumConfidence);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeHeader("PlatformDependent/Win/Bindings/SpeechBindings.h")]
	[NativeThrows]
	private static extern void Start_Internal(IntPtr recognizer);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeHeader("PlatformDependent/Win/Bindings/SpeechBindings.h")]
	private static extern void Stop_Internal(IntPtr recognizer);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeHeader("PlatformDependent/Win/Bindings/SpeechBindings.h")]
	private static extern bool IsRunning_Internal(IntPtr recognizer);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeHeader("PlatformDependent/Win/Bindings/SpeechBindings.h")]
	private static extern void Destroy(IntPtr recognizer);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ThreadSafe]
	[NativeHeader("PlatformDependent/Win/Bindings/SpeechBindings.h")]
	private static extern void DestroyThreaded(IntPtr recognizer);

	internal PhraseRecognizer()
	{
	}

	~PhraseRecognizer()
	{
		if (m_Recognizer != IntPtr.Zero)
		{
			DestroyThreaded(m_Recognizer);
			m_Recognizer = IntPtr.Zero;
			GC.SuppressFinalize(this);
		}
	}

	public void Start()
	{
		if (!(m_Recognizer == IntPtr.Zero))
		{
			Start_Internal(m_Recognizer);
		}
	}

	public void Stop()
	{
		if (!(m_Recognizer == IntPtr.Zero))
		{
			Stop_Internal(m_Recognizer);
		}
	}

	public void Dispose()
	{
		if (m_Recognizer != IntPtr.Zero)
		{
			Destroy(m_Recognizer);
			m_Recognizer = IntPtr.Zero;
		}
		GC.SuppressFinalize(this);
	}

	[RequiredByNativeCode]
	private void InvokePhraseRecognizedEvent(string text, ConfidenceLevel confidence, SemanticMeaning[] semanticMeanings, long phraseStartFileTime, long phraseDurationTicks)
	{
		this.OnPhraseRecognized?.Invoke(new PhraseRecognizedEventArgs(text, confidence, semanticMeanings, DateTime.FromFileTime(phraseStartFileTime), TimeSpan.FromTicks(phraseDurationTicks)));
	}

	[RequiredByNativeCode]
	private unsafe static SemanticMeaning[] MarshalSemanticMeaning(IntPtr keys, IntPtr values, IntPtr valueSizes, int valueCount)
	{
		SemanticMeaning[] array = new SemanticMeaning[valueCount];
		int num = 0;
		for (int i = 0; i < valueCount; i++)
		{
			uint num2 = ((uint*)(void*)valueSizes)[i];
			SemanticMeaning semanticMeaning = new SemanticMeaning
			{
				key = new string(((char**)(void*)keys)[i]),
				values = new string[num2]
			};
			for (int j = 0; j < num2; j++)
			{
				semanticMeaning.values[j] = new string(((char**)(void*)values)[num + j]);
			}
			array[i] = semanticMeaning;
			num += (int)num2;
		}
		return array;
	}
}
