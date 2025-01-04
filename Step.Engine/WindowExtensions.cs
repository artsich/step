using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace Step.Engine;

public static class WindowExtensions
{
	public static float GetDpi(this NativeWindow window)
	{
		float xScale;
		float yScale;

		unsafe
		{
			GLFW.GetWindowContentScale(window.WindowPtr, out xScale, out yScale);
		}

		var equal = Math.Abs(xScale - yScale) < 0.001f;

		// the above condition should always be true but who knows...
		Trace.Assert(equal, $"{nameof(GetDpi)}: mismatch between {nameof(xScale)} and {nameof(yScale)}");

		var scale = equal ? xScale : 1.0f;
		return scale;
	}

}
