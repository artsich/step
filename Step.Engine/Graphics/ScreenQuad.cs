using Silk.NET.OpenGL;

namespace Step.Engine.Graphics;

public class ScreenQuad : IDisposable
{
	private readonly uint _vao;
	private readonly GL GL;
	private bool _disposed;

	public ScreenQuad(GL gl)
	{
		GL = gl;
		_vao = GL.GenVertexArray();
	}

	public void Draw()
	{
		GL.BindVertexArray(_vao);
		GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed) return;

		if (disposing)
		{
			GL.DeleteVertexArray(_vao);
		}

		_disposed = true;
	}
}