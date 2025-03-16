using Silk.NET.OpenGL;
using StbImageSharp;

namespace Step.Engine.Graphics;

public class Texture2d : IDisposable
{
	private static GL GL => Ctx.GL;

	public uint Handle { get; private set; } = GL.GenTexture();

	public int Width { get; private set; }

	public int Height { get; private set; }

	public static Texture2d LoadFromStream(Stream stream)
	{
		var imageResult = ImageResult.FromStream(stream);

		var result = new Texture2d();
		result.SetImageData(
			imageResult.Width,
			imageResult.Height,
			InternalFormat.Rgba,
				imageResult.Comp switch
				{
					ColorComponents.Default => throw new NotImplementedException(),
					ColorComponents.Grey => throw new NotImplementedException(),
					ColorComponents.GreyAlpha => throw new NotImplementedException(),
					ColorComponents.RedGreenBlue => PixelFormat.Rgb,
					ColorComponents.RedGreenBlueAlpha => PixelFormat.Rgba,
					_ => throw new NotImplementedException()
				},
			imageResult.Data,
			false);

		return result;
	}

	public static Texture2d LoadFromFile(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new InvalidOperationException("[Texture 2d] - Nothing to load");
		}

		using var file = File.Open(path, FileMode.Open);
		return LoadFromStream(file);
	}

	public unsafe void SetImageData(
		int width,
		int height,
		InternalFormat internalFormat = InternalFormat.Rgba,
		PixelFormat sourceFormat = PixelFormat.Rgba,
		Span<byte> data = default,
		bool mipmap = true)
	{
		GL.BindTexture(TextureTarget.Texture2D, Handle);

		Width = width;
		Height = height;

		if (!data.IsEmpty)
		{
			fixed (void* d = &data[0])
			{
				GL.TexImage2D(
					TextureTarget.Texture2D,
					0,
					internalFormat,
					(uint)width, (uint)height,
					0,
					sourceFormat,
					PixelType.UnsignedByte,
					d);
			}
		}
		else
		{
			GL.TexImage2D(
				TextureTarget.Texture2D,
				0,
				internalFormat,
				(uint)width,
				(uint)height,
				0,
				sourceFormat,
				PixelType.UnsignedByte,
				null);
		}

		if (mipmap)
		{
			GL.GenerateMipmap(TextureTarget.Texture2D);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
		}

		SetWrap(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
		SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);

		GL.BindTexture(TextureTarget.Texture2D, 0);
	}

	public void SetFilter(TextureMinFilter minFilter, TextureMagFilter magFilter)
	{
		Bind();
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
	}

	public void SetWrap(TextureWrapMode s, TextureWrapMode t)
	{
		Bind();
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)s);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)t);
	}

	public void Bind(uint slot = 1)
	{
		GL.ActiveTexture(TextureUnit.Texture0 + (int)slot);
		GL.BindTexture(TextureTarget.Texture2D, Handle);
	}

	public void Unbind()
	{
		GL.BindTexture(TextureTarget.Texture2D, 0);
	}

	public void Dispose()
	{
		GL.DeleteTexture(Handle);
	}
}
