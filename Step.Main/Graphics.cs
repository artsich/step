using OpenTK.Graphics.OpenGL;

/*
 * Goals:
 *  Add score
 *  Render player Stats
 *  - inventory contains available effects of the user.
 *  - current health
 *  Helpers
 *  - God mode...
 *  
 *  Guns - pistol, knife - additional effects...
 */

namespace Step.Main;

public static class Graphics
{
	public static void PrintOpenGLInfo()
	{
		string vendor = GL.GetString(StringName.Vendor) ?? "Vendor not found...";
		string renderer = GL.GetString(StringName.Renderer) ?? "Renderer not found...";
		string version = GL.GetString(StringName.Version) ?? "Opengl version is not found...";
		string glslVersion = GL.GetString(StringName.ShadingLanguageVersion) ?? "GLSL version not found...";

		string extensionsStr = GL.GetString(StringName.Extensions) ?? "NotFound...";
		var extensions = extensionsStr.Split(' ');
		extensionsStr = string.Join('\n', extensions);

		Console.WriteLine("---------------------------------------");
		Console.WriteLine("OpenGL Information:");
		Console.WriteLine($"Vendor: {vendor}");
		Console.WriteLine($"Renderer: {renderer}");
		Console.WriteLine($"OpenGL Version: {version}");
		Console.WriteLine($"GLSL Version: {glslVersion}");
		Console.WriteLine($"Extensions:\n{extensionsStr}");
		Console.WriteLine("---------------------------------------");
	}
}
