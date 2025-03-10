﻿using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace Step.Engine.Graphics;

public class Texture2d(string path = "") : IDisposable
{
	public int Handle { get; private set; } = GL.GenTexture();

	public string Path { get; } = path;

	public int Width { get; private set; }

	public int Height { get; private set; }

	public Texture2d Load()
	{
		if (string.IsNullOrEmpty(Path))
		{
			throw new InvalidOperationException("[Texture 2d] - Nothing to load");
		}

		using var fileStream = File.OpenRead(Path);
		var imageResult = ImageResult.FromStream(fileStream);

		SetImageData(
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

		return this;
	}

	public unsafe void SetImageData(
		int width,
		int height,
		InternalFormat internalFormat = InternalFormat.Rgba,
		PixelFormat sourceFormat = PixelFormat.Rgba,
		Span<byte> data = default,
		bool mipmap = true)
	{
		GL.BindTexture(TextureTarget.Texture2d, Handle);

		Width = width;
		Height = height;

		if (!data.IsEmpty)
		{
			fixed (void* d = &data[0])
			{
				GL.TexImage2D(
					TextureTarget.Texture2d,
					0,
					internalFormat,
					width, height,
					0,
					sourceFormat,
					PixelType.UnsignedByte,
					d);
			}
		}
		else
		{
			GL.TexImage2D(
				TextureTarget.Texture2d,
				0,
				internalFormat,
				width,
				height,
				0,
				sourceFormat,
				PixelType.UnsignedByte,
				null);
		}

		if (mipmap)
		{
			GL.GenerateMipmap(TextureTarget.Texture2d);
			GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureBaseLevel, 0);
			GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMaxLevel, 8);
		}

		SetWrap(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
		SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);

		GL.BindTexture(TextureTarget.Texture2d, 0);
	}

	public void SetFilter(TextureMinFilter minFilter, TextureMagFilter magFilter)
	{
		Bind();
		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)minFilter);
		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)magFilter);
	}

	public void SetWrap(TextureWrapMode s, TextureWrapMode t)
	{
		Bind();
		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)s);
		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)t);
	}

	public void Bind(uint slot = 1)
	{
		GL.ActiveTexture(TextureUnit.Texture0 + slot);
		GL.BindTexture(TextureTarget.Texture2d, Handle);
	}

	public void Unbind()
	{
		GL.BindTexture(TextureTarget.Texture2d, 0);
	}

	public void Dispose()
	{
		GL.DeleteTexture(Handle);
	}
}
