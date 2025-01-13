using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Serilog;

namespace Step.Engine.Graphics;


public enum GeometryType : uint
{
	Quad = 0,
	Circle,
}

public record struct RenderCmd
{
	public int Layer;
	public GeometryType Type;

	public RenderTarget2d? Target;
	public Texture2d? Atlas;

	public Color4<Rgba> Color;
	public Rect? AtlasRect;
	public Matrix4 ModelMatrix;
}

public class Renderer(int screenWidth, int screenHeight)
{
	private Shader? _batchSpriteShader;
	private Shader _screenQuadShader;
	private ICamera2d? _camera;
	private Texture2d _defaultWhiteTexture;

	private readonly int _screenWidth = screenWidth;
	private readonly int _screenHeight = screenHeight;

	private readonly Stack<RenderTarget2d> _renderTargets = [];

	private readonly List<RenderCmd> _commands = [];

	private readonly SpriteBatch _spriteBatch = new();

	public void SetBackground(Color4<Rgba> color)
	{
		GL.ClearColor(color);
	}

	public void SetCamera(ICamera2d camera)
	{
		_camera = camera;
	}

	public void Load()
	{
		PrintOpenGLInfo();

		_batchSpriteShader = new Shader(
			"Assets/Shaders/SpriteBatch/shader.vert",
			"Assets/Shaders/SpriteBatch/shader.frag");

		Span<int> ids = stackalloc int[32];
		for (int i = 0; i < 32; i++)
		{
			ids[i] = i;
		}
		_batchSpriteShader.Set("diffuseTextures[0]", ids);

		_screenQuadShader = new Shader("Assets/Shaders/ScreenQuad/shader.vert", "Assets/Shaders/ScreenQuad/shader.frag");

		_defaultWhiteTexture = new Texture2d(".\\Assets\\Textures\\white.png").Load();
		_defaultWhiteTexture.Bind(0);
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

		DrawRect(position + shadowOffset, size, shadowColor, texture, layer: 1);
		DrawRect(position, size, color, texture, layer: 0);
	}

	public void DrawRect(
		Vector2 position,
		Vector2 size,
		Color4<Rgba> color,
		Texture2d? texture = null,
		int layer = 0)
	{
		var model = Matrix4.CreateScale(size.To3(1f)) * Matrix4.CreateTranslation(position.To3());

		var cmd = new RenderCmd
		{
			Atlas = texture,
			ModelMatrix = model,
			Color = color,
			Layer = layer,
			Type = GeometryType.Quad,
		};

		SubmitCommand(cmd);
	}

	public void DrawCircle(
		Vector2 position,
		float radius,
		Color4<Rgba> color,
		Texture2d? texture = null,
		int layer = 0)
	{
		var model = Matrix4.CreateScale(radius*2f) * Matrix4.CreateTranslation(position.To3());

		var cmd = new RenderCmd
		{
			Atlas = texture,
			ModelMatrix = model,
			Color = color,
			Layer = layer,
			Type = GeometryType.Circle,
		};

		SubmitCommand(cmd);
	}

	public void DrawScreenRectNow(Texture2d tex)
	{
		int vao = GL.GenVertexArray();
		_screenQuadShader!.Use();

		tex.Bind(0);
		_screenQuadShader.SetInt("diffuseTexture", 0);

		GL.BindVertexArray(vao);
		GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

		GL.DeleteVertexArray(vao);

		tex.Unbind();
	}

	public void Unload()
	{
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindVertexArray(0);
		GL.UseProgram(0);

		GL.DeleteProgram(_batchSpriteShader.Handle);
	}

	public void Flush()
	{
		int CompareTarget(RenderTarget2d? t1, RenderTarget2d? t2)
		{
			if (t1 == t2) return 0;
			if (t1 == null) return -1;
			if (t2 == null) return 1;
			return t1.Framebuffer.CompareTo(t2.Framebuffer);
		}

		_commands.Sort((a, b) =>
		{
			//int targetCompare = CompareTarget(b.Target, a.Target);
			//if (targetCompare != 0)
			//	return targetCompare;

			// todo: Why do i sort by layer if i can set Z position instead and use depth buffer for this?
			int layerCompare = b.Layer.CompareTo(a.Layer);
			if (layerCompare != 0)
				return layerCompare;

			int gTypeCompare = b.Type.CompareTo(a.Type);
			if (gTypeCompare != 0)
				return gTypeCompare;

			int aTexId = (a.Atlas == null) ? -1 : a.Atlas.Handle;
			int bTexId = (b.Atlas == null) ? -1 : b.Atlas.Handle;
			return bTexId.CompareTo(aTexId);
		});

		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(
			BlendingFactor.SrcAlpha,
			BlendingFactor.OneMinusSrcAlpha);

		_batchSpriteShader!.Use();
		_batchSpriteShader.SetMatrix4("viewProj", _camera!.ViewProj);

		foreach (var cmd in _commands)
		{
			_spriteBatch.AddSprite(
				cmd.ModelMatrix,
				cmd.Atlas!,
				geometryType: cmd.Type,
				textureRegion: cmd.AtlasRect,
				color: (Vector4)cmd.Color);
		}

		_spriteBatch.Flush();

		GL.Disable(EnableCap.Blend);
		_commands.Clear();
	}

	public void SubmitCommand(RenderCmd cmd)
	{
		cmd.Target = _renderTargets.Count > 0
					   ? _renderTargets.Peek()
					   : null;

		cmd.Atlas ??= _defaultWhiteTexture;

		_commands.Add(cmd);
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

		Log.Logger.Information("---------------------------------------");
		Log.Logger.Information("OpenGL Information:");
		Log.Logger.Information($"Vendor: {vendor}");
		Log.Logger.Information($"Renderer: {renderer}");
		Log.Logger.Information($"OpenGL Version: {version}");
		Log.Logger.Information($"GLSL Version: {glslVersion}");
		Log.Logger.Information($"Extensions:\n{extensionsStr}");
		Log.Logger.Information("---------------------------------------");
	}
}
