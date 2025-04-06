using Silk.NET.OpenGL;

namespace Step.Engine.Graphics.PostProcessing;

public class BlendPostEffect : ITextureBlendEffect
{
	private const string PathToResource = "Step.Engine.Graphics.PostProcessing.Shaders";

	private readonly ComputeShader _blendShader;
	private float _alphaThreshold = 0.99f;

	public float AlphaThreshold
	{
		get => _alphaThreshold;
		set => _alphaThreshold = Math.Clamp(value, 0f, 1f);
	}

	public BlendPostEffect()
	{
		_blendShader = ComputeShader.FromSource(
			EmbeddedResourceLoader.LoadAsString($"{PathToResource}.blend.glsl"));
	}

	public void Blend(Texture2d texture1, Texture2d texture2, out Texture2d output)
	{
		output = new Texture2d();
		output.SetImageData(
			texture1.Width,
			texture1.Height,
			InternalFormat.Rgba8,
			PixelFormat.Rgba,
			mipmap: false);

		_blendShader.Use();

		_blendShader.SetFloat("alphaThreshold", _alphaThreshold);

		var gl = Ctx.GL;
		
		gl.ActiveTexture(TextureUnit.Texture0);
		gl.BindTexture(TextureTarget.Texture2D, texture1.Handle);
		gl.ActiveTexture(TextureUnit.Texture1);
		gl.BindTexture(TextureTarget.Texture2D, texture2.Handle);
		
		gl.BindImageTexture(2, output.Handle, 0, false, 0, GLEnum.WriteOnly, InternalFormat.Rgba8);

		_blendShader.Dispatch(
			(uint)Math.Ceiling(output.Width / 8.0),
			(uint)Math.Ceiling(output.Height / 8.0),
			1
		);
	}
}