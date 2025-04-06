using Step.Engine.Editor;

namespace Step.Engine.Graphics.PostProcessing;

public sealed class CrtEffect(
	RenderTarget2d _renderTarget,
	Renderer _renderer) : IPostEffect, IDisposable
{
	private readonly Shader _shader = Shader.FromSource(
			EmbeddedResourceLoader.LoadAsString("Step.Engine.Graphics.Shaders.CRT.shader.vert"),
			EmbeddedResourceLoader.LoadAsString("Step.Engine.Graphics.Shaders.CRT.shader.frag"));

	private float _time;

	[EditorProperty(speed: 0.01f)]
	public float Distortion { get; set; } = 0.04f;

	[EditorProperty(speed: 0.0001f)]
	public float Dispersion { get; set; } = 0.0015f;

	[EditorProperty(speed: 0.01f)]
	public float BendScale { get; set; } = 1.5f;

	[EditorProperty(speed: 0.01f)]
	public float VignetteIntensity { get; set; } = 2.77f;

	[EditorProperty(speed: 0.01f)]
	public float VignetteRoundness { get; set; } = 0.5f;

	public Vector2f VignetteTarget { get; set; } = new(0.5f);

	public void Apply(Texture2d input, out Texture2d output)
	{
		_time += GameRoot.I.ScaledDt;

		_renderer.PushRenderTarget(_renderTarget);
		_shader.Use();

		_shader.SetFloat("time", _time);
		_shader.SetFloat("distortion", Distortion);
		_shader.SetFloat("bendScale", BendScale);
		_shader.SetFloat("dispersion", Dispersion);
		_shader.SetFloat("vignetteIntensity", VignetteIntensity);
		_shader.SetFloat("vignetteRoundness", VignetteRoundness);
		_shader.SetVector2("vignetteTarget", VignetteTarget);
		_shader.SetVector2("texSize", new(_renderTarget.Width, _renderTarget.Height));

		input.Bind(0);
		_shader.SetInt("sourceTexture", 0);

		_renderer.ScreenQuad.Draw();

		_renderer.PopRenderTarget();
		output = _renderTarget.Color;
	}

	public void DebugDraw()
	{
		EditOf.Render(this);
	}

	public void Dispose()
	{
		_shader.Dispose();
		_renderTarget.Dispose();
	}
}
