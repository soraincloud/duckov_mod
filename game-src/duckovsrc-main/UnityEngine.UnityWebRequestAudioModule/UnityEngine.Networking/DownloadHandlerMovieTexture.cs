using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Networking;

[StructLayout(LayoutKind.Sequential)]
[Obsolete("MovieTexture is deprecated. Use VideoPlayer instead.", true)]
public sealed class DownloadHandlerMovieTexture : DownloadHandler
{
	public MovieTexture movieTexture
	{
		get
		{
			FeatureRemoved();
			return null;
		}
	}

	public DownloadHandlerMovieTexture()
	{
		FeatureRemoved();
	}

	protected override byte[] GetData()
	{
		FeatureRemoved();
		return null;
	}

	protected override string GetText()
	{
		throw new NotSupportedException("String access is not supported for movies");
	}

	public static MovieTexture GetContent(UnityWebRequest uwr)
	{
		FeatureRemoved();
		return null;
	}

	private static void FeatureRemoved()
	{
		throw new Exception("Movie texture has been removed, use VideoPlayer instead");
	}
}
