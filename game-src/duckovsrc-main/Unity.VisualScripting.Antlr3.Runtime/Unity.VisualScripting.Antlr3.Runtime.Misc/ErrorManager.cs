using System;
using System.Diagnostics;
using System.Text;

namespace Unity.VisualScripting.Antlr3.Runtime.Misc;

public class ErrorManager
{
	public static void InternalError(object error, Exception e)
	{
		StackFrame lastNonErrorManagerCodeLocation = GetLastNonErrorManagerCodeLocation(e);
		string arg = string.Concat("Exception ", e, "@", lastNonErrorManagerCodeLocation, ": ", error);
		Error(arg);
	}

	public static void InternalError(object error)
	{
		StackFrame lastNonErrorManagerCodeLocation = GetLastNonErrorManagerCodeLocation(new Exception());
		string arg = string.Concat(lastNonErrorManagerCodeLocation, ": ", error);
		Error(arg);
	}

	private static StackFrame GetLastNonErrorManagerCodeLocation(Exception e)
	{
		StackTrace stackTrace = new StackTrace(e);
		int i;
		for (i = 0; i < stackTrace.FrameCount; i++)
		{
			StackFrame frame = stackTrace.GetFrame(i);
			if (frame.ToString().IndexOf("ErrorManager") < 0)
			{
				break;
			}
		}
		return stackTrace.GetFrame(i);
	}

	public static void Error(object arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("internal error: {0} ", arg);
	}
}
