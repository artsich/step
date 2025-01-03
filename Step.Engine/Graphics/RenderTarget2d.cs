using OpenTK.Graphics.OpenGL;

namespace Step.Engine.Graphics;

public class RenderTarget2d : IDisposable
{
	public int Framebuffer { get; private set; }

	public int Color { get; private set; }

	public int DepthStencil { get; private set; }

	public int Width { get; private set; }

	public int Height { get; private set; }

	public RenderTarget2d(int width, int height, bool useDepthStencil = false)
	{
		Initialize(width, height, useDepthStencil);
	}

	public void Resize(int newWidth, int newHeight, bool useDepthStencil = false)
	{
		if (newWidth == Width && newHeight == Height)
		{
			return;
		}

		DisposeAttachments();
		Initialize(newWidth, newHeight, useDepthStencil);
	}

	public void Begin()
	{
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, Framebuffer);
		GL.Viewport(0, 0, Width, Height);
	}

	public void End(int screenWidth, int screenHeight)
	{
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		GL.Viewport(0, 0, screenWidth, screenHeight);
	}

	public void Dispose()
	{
		DisposeAttachments();

		if (Framebuffer != 0)
		{
			GL.DeleteFramebuffer(Framebuffer);
			Framebuffer = 0;
		}
	}

	private void DisposeAttachments()
	{
		if (Color != 0)
		{
			GL.DeleteTexture(Color);
			Color = 0;
		}

		if (DepthStencil != 0)
		{
			GL.DeleteRenderbuffer(DepthStencil);
			DepthStencil = 0;
		}
	}

	private void Initialize(int width, int height, bool useDepthStencil)
	{
		Width = width;
		Height = height;

		Framebuffer = GL.GenFramebuffer();
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, Framebuffer);

		Color = CreateColorTexture(width, height);
		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, Color, 0);

		if (useDepthStencil)
		{
			DepthStencil = CreateDepthStencil(width, height);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, DepthStencil);
		}

		var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
		if (status != FramebufferStatus.FramebufferComplete)
			throw new Exception("Framebuffer not complete: " + status);

		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	}

	private static int CreateDepthStencil(int width, int height)
	{
		var depthStencil = GL.GenRenderbuffer();
		GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthStencil);
		GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, width, height);
		return depthStencil;
	}

	private static int CreateColorTexture(int width, int height)
	{
		var color = GL.GenTexture();
		GL.BindTexture(TextureTarget.Texture2d, color);
		GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, nint.Zero);
		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
		return color;
	}
}
