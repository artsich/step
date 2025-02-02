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

	public Vector2 Pivot = new(0.5f);

	public RenderTarget2d? Target;
	public Texture2d? Atlas;
	public Shader? Shader;

	public Color4<Rgba> Color;
	public Rect? AtlasRect;
	public Matrix4 ModelMatrix;

	public RenderCmd()
	{
	}
}

public struct RenderStats
{
	public int TexturesUsed;
	public int DrawCalls;
	public int TotalSprites;
	public float GpuTimeMs;
	public int ActiveShaders;

	public void Reset()
	{
		TexturesUsed = 0;
		DrawCalls = 0;
		TotalSprites = 0;
		GpuTimeMs = 0;
		ActiveShaders = 0;
	}
}

public class Renderer(int screenWidth, int screenHeight)
{
	private Shader? _batchSpriteShader;
	private Shader? _screenQuadShader;
	private ICamera2d? _camera;
	public Texture2d DefaultWhiteTexture { get; private set; }

	private readonly int _screenWidth = screenWidth;
	private readonly int _screenHeight = screenHeight;

	private readonly Stack<RenderTarget2d> _renderTargets = [];

	private readonly List<RenderCmd> _commands = [];

	private readonly SpriteBatch _spriteBatch = new();

	public ScreenQuad ScreenQuad { get; } = new ScreenQuad();

	public RenderStats Stats;
	private GpuTimer _gpuTimer = new();

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

		_screenQuadShader = new Shader(
			"Assets/Shaders/ScreenQuad/shader.vert",
			"Assets/Shaders/ScreenQuad/shader.frag");

		DefaultWhiteTexture = new Texture2d(".\\Assets\\Textures\\white.png").Load();
		DefaultWhiteTexture.Bind(0);
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
		var model = Matrix4.CreateScale(radius * 2f) * Matrix4.CreateTranslation(position.To3());

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
		_screenQuadShader!.Use();

		tex.Bind(0);
		_screenQuadShader.SetInt("diffuseTexture", 0);

		ScreenQuad.Draw();

		tex.Unbind();
	}

	public void Unload()
	{
		_gpuTimer.Dispose();

		GL.UseProgram(0);
		GL.DeleteProgram(_batchSpriteShader!.Handle);
	}

	public void Flush()
	{
		Stats.Reset();

		//todo: Investigate how much allocations happens here!
		_commands.Sort(CompareRenderCommands);

		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(
			BlendingFactor.SrcAlpha,
			BlendingFactor.OneMinusSrcAlpha);

		_gpuTimer.Start();

		Shader currentShader = _batchSpriteShader!;
		SetDefaultShaderVariables(currentShader);

		foreach (var cmd in _commands)
		{
			if (cmd.Shader != currentShader)
			{
				_spriteBatch.Flush();
				currentShader = cmd.Shader!;
				currentShader.Use();
				SetDefaultShaderVariables(currentShader);
			}

			_spriteBatch.AddSprite(
				cmd.ModelMatrix,
				cmd.Atlas!,
				pivot: cmd.Pivot,
				geometryType: cmd.Type,
				textureRegion: cmd.AtlasRect,
				color: (Vector4)cmd.Color);
		}

		_spriteBatch.Flush();
		GL.Disable(EnableCap.Blend);

		Stats.GpuTimeMs = _gpuTimer.Stop();

		_commands.Clear();
	}

	public void SubmitCommand(RenderCmd cmd)
	{
		cmd.Target = _renderTargets.Count > 0
					   ? _renderTargets.Peek()
					   : null;

		cmd.Atlas ??= DefaultWhiteTexture;
		cmd.Shader ??= _batchSpriteShader;

		_commands.Add(cmd);
	}

	private void SetDefaultShaderVariables(Shader shader)
	{
		shader.SetMatrix4("viewProj", _camera!.ViewProj);

		Span<int> ids = stackalloc int[32];
		for (int i = 0; i < 32; i++)
		{
			ids[i] = i;
		}
		shader.Set("diffuseTextures[0]", ids);
	}

	private static int CompareTarget(RenderTarget2d? t1, RenderTarget2d? t2)
	{
		if (t1 == t2) return 0;
		if (t1 == null) return -1;
		if (t2 == null) return 1;
		return t1.Framebuffer.CompareTo(t2.Framebuffer);
	}

	private static int CompareRenderCommands(RenderCmd a, RenderCmd b)
	{
		//int targetCompare = CompareTarget(b.Target, a.Target);
		//if (targetCompare != 0)
		//    return targetCompare;

		int layerCompare = a.Layer.CompareTo(b.Layer);
		if (layerCompare != 0)
			return layerCompare;

		int shaderCompare = a.Shader!.Handle.CompareTo(b.Shader!.Handle);
		if (shaderCompare != 0)
			return shaderCompare;

		int gTypeCompare = b.Type.CompareTo(a.Type);
		if (gTypeCompare != 0)
			return gTypeCompare;

		int aTexId = a.Atlas == null ? -1 : a.Atlas.Handle;
		int bTexId = b.Atlas == null ? -1 : b.Atlas.Handle;
		return bTexId.CompareTo(aTexId);
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
