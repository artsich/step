using Silk.NET.OpenGL;

namespace Step.Engine.Graphics.PostProcessing;

public class BlendPostEffect : ITextureBlendEffect
{
	private const string PathToResource = "Step.Engine.Graphics.PostProcessing.Shaders";

	private readonly ComputeShader _blendShader;
	private readonly Texture2d _result;
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

		_result = new Texture2d();
	}

	public void Blend(Texture2d texture1, Texture2d texture2, out Texture2d output)
	{
		if (texture1.Width != texture2.Width || texture1.Height != texture2.Height)
		{
			throw new InvalidOperationException("Blending not available, different texture size.");
		}

		if (texture1.Width != _result.Width || texture1.Height != _result.Height)
		{
			_result.SetImageData(
				texture1.Width,
				texture1.Height,
				InternalFormat.Rgba8,
				PixelFormat.Rgba,
				mipmap: false);
		}

		_blendShader.Use();

		_blendShader.SetFloat("alphaThreshold", _alphaThreshold);

		var gl = Ctx.GL;
		
		gl.ActiveTexture(TextureUnit.Texture0);
		gl.BindTexture(TextureTarget.Texture2D, texture1.Handle);
		gl.ActiveTexture(TextureUnit.Texture1);
		gl.BindTexture(TextureTarget.Texture2D, texture2.Handle);
		
		gl.BindImageTexture(2, _result.Handle, 0, false, 0, GLEnum.WriteOnly, InternalFormat.Rgba8);

		_blendShader.Dispatch(
			(uint)Math.Ceiling(_result.Width / 8.0),
			(uint)Math.Ceiling(_result.Height / 8.0),
			1
		);

		output = _result;
	}
}