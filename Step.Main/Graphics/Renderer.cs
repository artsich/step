using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Step.Main.Graphics;

public class Renderer(int screenWidth, int screenHeight)
{
	private int _vao;
	private Shader? _shader;
	private Camera2d? _camera;
	private Texture2d _defaultWhiteTexture;

	private int _screenWidth = screenWidth;
	private int _screenHeight = screenHeight;

	private readonly Stack<RenderTarget2d> _renderTargets = [];

	public void SetBackground(Color4<Rgba> color)
	{
		GL.ClearColor(color);
	}

	public void SetCamera(Camera2d camera)
	{
		_camera = camera;
	}

	public void Load()
	{
		PrintOpenGLInfo();
		_vao = GL.GenVertexArray();

		_shader = new Shader("Assets/Shaders/shader.vert", "Assets/Shaders/shader.frag");
		_defaultWhiteTexture = new Texture2d(".\\Assets\\Textures\\white.png").Load();
		_defaultWhiteTexture.BindAsSampler(0);

		GL.Disable(EnableCap.CullFace);
		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
	}

	public void PushRenderTarget(RenderTarget2d renderTarget)
	{
		renderTarget.Begin();
		_renderTargets.Push(renderTarget);
	}

	public void PopRenderTarget()
	{
		if (_renderTargets.Count == 0)
		{
			return;
		}

		_renderTargets.Pop().End(_screenWidth, _screenHeight);

		if (_renderTargets.Count > 0)
		{
			_renderTargets.Peek().Begin();
		}
	}

	public void DrawObject(Vector2 position, Vector2 size, Color4<Rgba> color, Texture2d? texture = null)
	{
		Vector2 shadowOffset = new(1, -1);
		Color4<Rgba> shadowColor = new(0f, 0f, 0f, 0.25f);

		DrawRect(position + shadowOffset, size, shadowColor, texture);
		DrawRect(position, size, color, texture);
	}

	public void DrawRect(Vector2 position, Vector2 size, Color4<Rgba> color, Texture2d? texture = null)
	{
		_shader!.Use();
		_shader.SetMatrix4("viewProj", _camera!.ViewProj);

		var model = Matrix4.CreateScale(size.To3(1f)) * Matrix4.CreateTranslation(position.To3());
		_shader.SetMatrix4("model", model);
		_shader.SetColor("color", color);

		if (texture != null)
		{
			texture?.BindAsSampler(1);
			_shader.SetInt("diffuseTexture", 1);
		}
		else
		{
			_defaultWhiteTexture?.BindAsSampler(0);
			_shader.SetInt("diffuseTexture", 0);
		}

		GL.BindVertexArray(_vao);
		GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
		texture?.Unbind();
	}

	public void Unload()
	{
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindVertexArray(0);
		GL.UseProgram(0);

		GL.DeleteProgram(_shader.Handle);
	}

	private static void PrintOpenGLInfo()
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
