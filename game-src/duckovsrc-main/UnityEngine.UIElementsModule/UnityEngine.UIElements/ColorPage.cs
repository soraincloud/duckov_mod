using UnityEngine.UIElements.UIR;

namespace UnityEngine.UIElements;

internal struct ColorPage
{
	public bool isValid;

	public Color32 pageAndID;

	public static ColorPage Init(RenderChain renderChain, BMPAlloc alloc)
	{
		bool flag = alloc.IsValid();
		return new ColorPage
		{
			isValid = flag,
			pageAndID = (flag ? renderChain.shaderInfoAllocator.ColorAllocToVertexData(alloc) : default(Color32))
		};
	}

	public MeshBuilderNative.NativeColorPage ToNativeColorPage()
	{
		return new MeshBuilderNative.NativeColorPage
		{
			isValid = (isValid ? 1 : 0),
			pageAndID = pageAndID
		};
	}
}
