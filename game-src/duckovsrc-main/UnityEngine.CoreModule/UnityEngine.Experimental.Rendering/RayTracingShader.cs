using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Experimental.Rendering;

[NativeHeader("Runtime/Shaders/RayTracingShader.h")]
[NativeHeader("Runtime/Graphics/ShaderScriptBindings.h")]
[NativeHeader("Runtime/Shaders/RayTracingAccelerationStructure.h")]
public sealed class RayTracingShader : Object
{
	public extern float maxRecursionDepth
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingShaderScripting::SetFloat", HasExplicitThis = true)]
	public extern void SetFloat(int nameID, float val);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingShaderScripting::SetInt", HasExplicitThis = true)]
	public extern void SetInt(int nameID, int val);

	[FreeFunction(Name = "RayTracingShaderScripting::SetVector", HasExplicitThis = true)]
	public void SetVector(int nameID, Vector4 val)
	{
		SetVector_Injected(nameID, ref val);
	}

	[FreeFunction(Name = "RayTracingShaderScripting::SetMatrix", HasExplicitThis = true)]
	public void SetMatrix(int nameID, Matrix4x4 val)
	{
		SetMatrix_Injected(nameID, ref val);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingShaderScripting::SetFloatArray", HasExplicitThis = true)]
	private extern void SetFloatArray(int nameID, float[] values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingShaderScripting::SetIntArray", HasExplicitThis = true)]
	private extern void SetIntArray(int nameID, int[] values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingShaderScripting::SetVectorArray", HasExplicitThis = true)]
	public extern void SetVectorArray(int nameID, Vector4[] values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingShaderScripting::SetMatrixArray", HasExplicitThis = true)]
	public extern void SetMatrixArray(int nameID, Matrix4x4[] values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "RayTracingShaderScripting::SetTexture", HasExplicitThis = true, IsFreeFunction = true)]
	public extern void SetTexture(int nameID, [NotNull("ArgumentNullException")] Texture texture);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "RayTracingShaderScripting::SetBuffer", HasExplicitThis = true, IsFreeFunction = true)]
	public extern void SetBuffer(int nameID, [NotNull("ArgumentNullException")] ComputeBuffer buffer);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "RayTracingShaderScripting::SetBuffer", HasExplicitThis = true, IsFreeFunction = true)]
	private extern void SetGraphicsBuffer(int nameID, [NotNull("ArgumentNullException")] GraphicsBuffer buffer);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingShaderScripting::SetConstantBuffer", HasExplicitThis = true)]
	private extern void SetConstantComputeBuffer(int nameID, [NotNull("ArgumentNullException")] ComputeBuffer buffer, int offset, int size);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingShaderScripting::SetConstantBuffer", HasExplicitThis = true)]
	private extern void SetConstantGraphicsBuffer(int nameID, [NotNull("ArgumentNullException")] GraphicsBuffer buffer, int offset, int size);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "RayTracingShaderScripting::SetAccelerationStructure", HasExplicitThis = true, IsFreeFunction = true)]
	public extern void SetAccelerationStructure(int nameID, [NotNull("ArgumentNullException")] RayTracingAccelerationStructure accelerationStructure);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetShaderPass(string passName);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "RayTracingShaderScripting::SetTextureFromGlobal", HasExplicitThis = true, IsFreeFunction = true)]
	public extern void SetTextureFromGlobal(int nameID, int globalTextureNameID);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("DispatchRays")]
	public extern void Dispatch(string rayGenFunctionName, int width, int height, int depth, Camera camera = null);

	public void SetBuffer(int nameID, GraphicsBuffer buffer)
	{
		SetGraphicsBuffer(nameID, buffer);
	}

	private RayTracingShader()
	{
	}

	public void SetFloat(string name, float val)
	{
		SetFloat(Shader.PropertyToID(name), val);
	}

	public void SetInt(string name, int val)
	{
		SetInt(Shader.PropertyToID(name), val);
	}

	public void SetVector(string name, Vector4 val)
	{
		SetVector(Shader.PropertyToID(name), val);
	}

	public void SetMatrix(string name, Matrix4x4 val)
	{
		SetMatrix(Shader.PropertyToID(name), val);
	}

	public void SetVectorArray(string name, Vector4[] values)
	{
		SetVectorArray(Shader.PropertyToID(name), values);
	}

	public void SetMatrixArray(string name, Matrix4x4[] values)
	{
		SetMatrixArray(Shader.PropertyToID(name), values);
	}

	public void SetFloats(string name, params float[] values)
	{
		SetFloatArray(Shader.PropertyToID(name), values);
	}

	public void SetFloats(int nameID, params float[] values)
	{
		SetFloatArray(nameID, values);
	}

	public void SetInts(string name, params int[] values)
	{
		SetIntArray(Shader.PropertyToID(name), values);
	}

	public void SetInts(int nameID, params int[] values)
	{
		SetIntArray(nameID, values);
	}

	public void SetBool(string name, bool val)
	{
		SetInt(Shader.PropertyToID(name), val ? 1 : 0);
	}

	public void SetBool(int nameID, bool val)
	{
		SetInt(nameID, val ? 1 : 0);
	}

	public void SetTexture(string name, Texture texture)
	{
		SetTexture(Shader.PropertyToID(name), texture);
	}

	public void SetBuffer(string name, ComputeBuffer buffer)
	{
		SetBuffer(Shader.PropertyToID(name), buffer);
	}

	public void SetBuffer(string name, GraphicsBuffer buffer)
	{
		SetBuffer(Shader.PropertyToID(name), buffer);
	}

	public void SetConstantBuffer(int nameID, ComputeBuffer buffer, int offset, int size)
	{
		SetConstantComputeBuffer(nameID, buffer, offset, size);
	}

	public void SetConstantBuffer(string name, ComputeBuffer buffer, int offset, int size)
	{
		SetConstantComputeBuffer(Shader.PropertyToID(name), buffer, offset, size);
	}

	public void SetConstantBuffer(int nameID, GraphicsBuffer buffer, int offset, int size)
	{
		SetConstantGraphicsBuffer(nameID, buffer, offset, size);
	}

	public void SetConstantBuffer(string name, GraphicsBuffer buffer, int offset, int size)
	{
		SetConstantGraphicsBuffer(Shader.PropertyToID(name), buffer, offset, size);
	}

	public void SetAccelerationStructure(string name, RayTracingAccelerationStructure accelerationStructure)
	{
		SetAccelerationStructure(Shader.PropertyToID(name), accelerationStructure);
	}

	public void SetTextureFromGlobal(string name, string globalTextureName)
	{
		SetTextureFromGlobal(Shader.PropertyToID(name), Shader.PropertyToID(globalTextureName));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetVector_Injected(int nameID, ref Vector4 val);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetMatrix_Injected(int nameID, ref Matrix4x4 val);
}
