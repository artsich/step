using Silk.NET.OpenGL;

namespace Step.Engine.Graphics;

public sealed class BlurEffect : IDisposable, IPostEffect
{
	private readonly ComputeShader _blurShader;
	private readonly Texture2d _tempTexture;
	private readonly uint _outputImageBinding = 1;

	public float Quality { get; set; } = 3.0f;
	public float Directions { get; set; } = 16.0f;
	public float Size { get; set; } = 8.0f;

	public BlurEffect()
	{
		_blurShader = ComputeShader.FromSource(
			EmbeddedResourceLoader.LoadAsString("Step.Engine.Graphics.Shaders.blur.glsl"));

		_tempTexture = new Texture2d();
	}

	public void Apply(Texture2d sourceTexture, out Texture2d output)
	{
		var gl = Ctx.GL;
		
		if (_tempTexture.Width != sourceTexture.Width || _tempTexture.Height != sourceTexture.Height)
		{
			_tempTexture.SetImageData(
				sourceTexture.Width,
				sourceTexture.Height,
				InternalFormat.Rgba8,
				PixelFormat.Rgba,
				mipmap: false);
		}

		_blurShader.Use();
		_blurShader.SetFloat("Quality", Quality);
		_blurShader.SetFloat("Directions", Directions);
		_blurShader.SetFloat("Size", Size);
		gl.ActiveTexture(TextureUnit.Texture0);
		gl.BindTexture(TextureTarget.Texture2D, sourceTexture.Handle);
		gl.BindImageTexture(_outputImageBinding, _tempTexture.Handle, 0, false, 0, GLEnum.WriteOnly, InternalFormat.Rgba8);

		uint groupSizeX = 8;
		uint groupSizeY = 8;
		uint numGroupsX = (uint)Math.Ceiling(sourceTexture.Width / (float)groupSizeX);
		uint numGroupsY = (uint)Math.Ceiling(sourceTexture.Height / (float)groupSizeY);
		
		_blurShader.Dispatch(numGroupsX, numGroupsY, 1);
		_blurShader.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);

		output = _tempTexture;
	}

	public void Dispose()
	{
		_blurShader.Dispose();
		_tempTexture.Dispose();
	}

	public void DebugDraw()
	{
		throw new NotImplementedException();
	}
} 