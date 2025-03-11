using Silk.NET.OpenGL;
using System.Reflection.Metadata;

namespace Step.Engine.Graphics;

public class RenderTarget2d : IDisposable
{
	private readonly GL GL;

	public uint Framebuffer { get; private set; }

	public Texture2d Color { get; private set; }

	public uint DepthStencil { get; private set; }

	public int Width { get; private set; }

	public int Height { get; private set; }

	public RenderTarget2d(int width, int height, bool useDepthStencil = false)
	{
		GL = Ctx.GL;
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
		GL.Viewport(0, 0, (uint)Width, (uint)Height);
	}

	public void End(int screenWidth, int screenHeight)
	{
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		GL.Viewport(0, 0, (uint)screenWidth, (uint)screenHeight);
	}

	public void Clear(Vector4f color)
	{
		GL.GetInteger(GLEnum.FramebufferBinding, out var previousFramebuffer);

		if (previousFramebuffer != Framebuffer)
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, Framebuffer);
			GL.ClearColor(color.X, color.Y, color.Z, color.W);
			GL.Clear(ClearBufferMask.ColorBufferBit
				| ClearBufferMask.DepthBufferBit
				| ClearBufferMask.StencilBufferBit);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, (uint)previousFramebuffer);
		}
		else
		{
			GL.ClearColor(color.X, color.Y, color.Z, color.W);
			GL.Clear(ClearBufferMask.ColorBufferBit
				| ClearBufferMask.DepthBufferBit
				| ClearBufferMask.StencilBufferBit);
		}
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
		Color?.Dispose();

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
		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Color.Handle, 0);

		if (useDepthStencil)
		{
			DepthStencil = CreateDepthStencil(width, height);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, DepthStencil);
		}

		var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
		if (status != GLEnum.FramebufferComplete)
			throw new Exception("Framebuffer not complete: " + status);

		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	}

	private uint CreateDepthStencil(int width, int height)
	{
		var depthStencil = GL.GenRenderbuffer();
		GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthStencil);
		GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, (uint)width, (uint)height);
		return depthStencil;
	}

	private static Texture2d CreateColorTexture(int width, int height)
	{
		var result = new Texture2d();
		result.SetImageData(
			width, height,
			InternalFormat.Rgba8,
			PixelFormat.Rgba,
			[]);

		result.SetWrap(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
		result.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
	
		return result;
	}
}
