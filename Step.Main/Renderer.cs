using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Step.Main;

public class Renderer
{
	private readonly float[] _rectVertices =
	{
		// Position          // Texture Coordinates
		-0.5f, -0.5f, 0.0f,  0.0f, 0.0f,  // Bottom-left corner
		 0.5f, -0.5f, 0.0f,  1.0f, 0.0f,  // Bottom-right corner
		 0.5f,  0.5f, 0.0f,  1.0f, 1.0f,  // Top-right corner
		-0.5f,  0.5f, 0.0f,  0.0f, 1.0f   // Top-left corner
	};
	private int _vertexBufferObject;
	private int _vertexArrayObject;
	private Shader _shader;
	private Camera2d _camera;

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

		_vertexBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

		GL.BufferData(BufferTarget.ArrayBuffer, _rectVertices.Length * sizeof(float), _rectVertices, BufferUsage.StaticDraw);

		_vertexArrayObject = GL.GenVertexArray();
		GL.BindVertexArray(_vertexArrayObject);

		// Position attribute
		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
		GL.EnableVertexAttribArray(0);

		// Texture coordinate attribute
		GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
		GL.EnableVertexAttribArray(1);

		_shader = new Shader("Assets/Shaders/shader.vert", "Assets/Shaders/shader.frag");
	}

	public void DrawObject(Vector2 position, Vector2 size, Color4<Rgba> color, Texture2d? texture = null)
	{
		Vector2 shadowOffset = new(1, -1);
		Color4<Rgba> shadowColor = new(0f, 0f, 0f, 0.25f);

		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		DrawRect(position + shadowOffset, size, shadowColor, texture);

		DrawRect(position, size, color, texture);

		GL.Disable(EnableCap.Blend);
	}

	private void DrawRect(Vector2 position, Vector2 size, Color4<Rgba> color, Texture2d? texture = null)
	{
		_shader.Use();
		_shader.SetMatrix4("viewProj", _camera.ViewProj);

		var model = Matrix4.CreateScale(size.To3(1f)) * Matrix4.CreateTranslation(position.To3());
		_shader.SetMatrix4("model", model);
		_shader.SetColor("color", color);

		if (texture != null)
		{
			texture?.BindAsSampler(0);
			_shader.SetInt("diffuseTexture", 0);
		}

		GL.BindVertexArray(_vertexArrayObject);
		GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

		texture?.Unbind();
	}

	public void Unload()
	{
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindVertexArray(0);
		GL.UseProgram(0);

		GL.DeleteBuffer(_vertexBufferObject);
		GL.DeleteVertexArray(_vertexArrayObject);

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
