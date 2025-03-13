using Serilog;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Runtime.InteropServices;

namespace Step.Engine.Graphics;

public class Ctx
{
	public static GL GL;

	public static void Init(IWindow window)
	{
		GL = window.CreateOpenGL();

		if (OperatingSystem.IsWindows())
		{
			GL.Enable(EnableCap.DebugOutput);
			GL.Enable(EnableCap.DebugOutputSynchronous);
			GL.DebugMessageCallback(GlDebugCallback.FuncPtr, in IntPtr.Zero);
		}
	}

	public static void PrintOpenGLInfo()
	{
		string vendor = GL.GetStringS(StringName.Vendor) ?? "Vendor not found...";
		string renderer = GL.GetStringS(StringName.Renderer) ?? "Renderer not found...";
		string version = GL.GetStringS(StringName.Version) ?? "Opengl version is not found...";
		string glslVersion = GL.GetStringS(StringName.ShadingLanguageVersion) ?? "GLSL version not found...";

		// TODO: OpenGL: GL_INVALID_ENUM error generated.
		string extensionsStr = GL.GetStringS(StringName.Extensions) ?? "NotFound...";
		var extensions = extensionsStr.Split(' ');
		extensionsStr = string.Join('\n', extensions);

		Log.Logger.Information("---------------------------------------");
		Log.Logger.Information("OpenGL Information:");
		Log.Logger.Information($"Vendor: {vendor}");
		Log.Logger.Information($"Renderer: {renderer}");
		Log.Logger.Information($"OpenGL Version: {version}");
		Log.Logger.Information($"GLSL Version: {glslVersion}");
		Log.Logger.Information($"Extensions:\n{extensionsStr}");
		Log.Logger.Information("---------------------------------------");
	}

	private static class GlDebugCallback
	{
		public readonly static DebugProc FuncPtr = GLDebugCallback;
		private const uint GL_DEBUG_CALLBACK_APP_MAKER_ID = 0;

		private static void GLDebugCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userParam)
		{
			if (source == GLEnum.DebugSourceApplication && id == GL_DEBUG_CALLBACK_APP_MAKER_ID)
			{
				return;
			}

			string text = "OpenGL: " + Marshal.PtrToStringAnsi(message, length);
			switch (severity)
			{
				case GLEnum.DebugSeverityLow:
					Log.Logger.Information(text);
					break;
				case GLEnum.DebugSeverityMedium:
					Log.Logger.Warning(text);
					break;
				case GLEnum.DebugSeverityHigh:
					Log.Logger.Error(text);
					break;
				case GLEnum.DebugSeverityNotification:
					if (id == 131185) return; // Buffer detailed info, NVIDIA
					Log.Logger.Information(text);
					break;
				case GLEnum.DontCare:
				default:
					break;
			}
		}
	}
}
