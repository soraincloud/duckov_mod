using UnityEngine.InputSystem;

namespace UnityEngine.Rendering;

public static class ShaderDebugPrintInputProducer
{
	public static ShaderDebugPrintInput Get()
	{
		ShaderDebugPrintInput result = default(ShaderDebugPrintInput);
		result.pos = Input.mousePosition;
		result.leftDown = Input.GetMouseButton(0);
		result.rightDown = Input.GetMouseButton(1);
		result.middleDown = Input.GetMouseButton(2);
		Mouse current = Mouse.current;
		result.pos = current.position.ReadValue();
		result.leftDown = current.leftButton.isPressed;
		result.rightDown = current.rightButton.isPressed;
		result.middleDown = current.middleButton.isPressed;
		return result;
	}
}
