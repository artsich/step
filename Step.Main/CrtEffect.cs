using OpenTK.Mathematics;
using Step.Engine;
using Step.Engine.Editor;
using Step.Engine.Graphics;

namespace Step.Main;

public class CrtEffect(
	Shader shader,
	RenderTarget2d renderTarget,
	Renderer renderer
) : IPostEffect
{
	private float _time;

	[EditorProperty(speed: 0.01f)]
	public float Distortion { get; set; } = 0.04f;

	[EditorProperty(speed: 0.0001f)]
	public float Dispersion { get; set; } = 0.0015f;

	[EditorProperty(speed: 0.01f)]
	public float BendScale { get; set; } = 1.5f;

	[EditorProperty(speed: 0.01f)]
	public float VignetteIntensity { get; set; } = 1.5f;

	[EditorProperty(speed: 0.01f)]
	public float VignetteRoundness { get; set; } = 0.5f;

	public Vector2 VignetteTarget { get; set; } = new Vector2(0.5f);

	public void Apply(Texture2d input, out Texture2d output)
	{
		_time += GameRoot.I.ScaledDt;

		renderer.PushRenderTarget(renderTarget);
		shader.Use();

		shader.SetFloat("time", _time);
		shader.SetFloat("distortion", Distortion);
		shader.SetFloat("bendScale", BendScale);
		shader.SetFloat("dispersion", Dispersion);
		shader.SetFloat("vignetteIntensity", VignetteIntensity);
		shader.SetFloat("vignetteRoundness", VignetteRoundness);
		shader.SetVector2("vignetteTarget", VignetteTarget);
		shader.SetVector2("texSize", new Vector2(renderTarget.Width, renderTarget.Height));

		input.Bind(0);
		shader.SetInt("sourceTexture", 0);

		renderer.ScreenQuad.Draw();

		renderer.PopRenderTarget();
		output = renderTarget.Color;
	}

	public void DebugDraw()
	{
		EditOf.Render(this);
	}
}
