using Silk.NET.GLFW;
using System.Diagnostics;

namespace Step.Engine;

public static class WindowExt
{
	public static float GetScale()
	{
		var glfw = Glfw.GetApi();
		float xScale;
		float yScale;

		unsafe
		{
			glfw.GetMonitorContentScale(glfw.GetPrimaryMonitor(), out xScale, out yScale);
		}

		var equal = Math.Abs(xScale - yScale) < 0.001f;
		Trace.Assert(equal, $"{nameof(GetScale)}: mismatch between {nameof(xScale)} and {nameof(yScale)}");

		var scale = equal ? xScale : 1.0f;
		return scale;
	}
}
