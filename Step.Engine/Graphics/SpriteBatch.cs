using Silk.NET.OpenGL;
using System.Runtime.CompilerServices;

namespace Step.Engine.Graphics;

public sealed class SpriteBatch : IDisposable
{
	private record struct SpriteVertex(
		int TexId,
		int GType,
		Vector2f Position,
		Vector2f TexCoord,
		Vector4f Color);

	private const int QuadVerticesCount = 4;

	private const int MaxQuads = 100_000;
	private const int IndicesPerQuad = 6;
	private uint[] _indices;

	private uint _verticesUsed;
	private readonly SpriteVertex[] _vertices;
	private int _textureUsed;
	private readonly Texture2d[] _textures;

	private readonly Vector4f[] QuadVertices =
	{
		new(0f, 0f, 0f, 1f),
		new(1f, 0f, 0f, 1f),
		new(1f, 1f, 0f, 1f),
		new(0f, 1f, 0f, 1f)
	};

	private uint _vao, _vbo, _ebo;

	public readonly int MaxTextures;
	private readonly GL GL = Ctx.GL;

	public SpriteBatch()
	{
		GL.GetInteger(GetPName.MaxTextureImageUnits, out MaxTextures);
		_textures = new Texture2d[MaxTextures];
		_vertices = new SpriteVertex[MaxQuads * QuadVerticesCount];

		InitializeBuffers();
	}

	private unsafe void InitializeBuffers()
	{
		_indices = new uint[MaxQuads * IndicesPerQuad];

		uint uboOffset = 0;
		for (int i = 0; i < MaxQuads; i++)
		{
			_indices[i * 6 + 0] = uboOffset + 0;
			_indices[i * 6 + 1] = uboOffset + 1;
			_indices[i * 6 + 2] = uboOffset + 2;
			_indices[i * 6 + 3] = uboOffset + 2;
			_indices[i * 6 + 4] = uboOffset + 3;
			_indices[i * 6 + 5] = uboOffset + 0;

			uboOffset += 4;
		}

		_vao = GL.GenVertexArray();
		GL.BindVertexArray(_vao);

		_ebo = GL.GenBuffer();
		GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

		GL.BufferData<uint>(
			BufferTargetARB.ElementArrayBuffer,
			_indices,
			BufferUsageARB.StaticDraw
		);

		_vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
		GL.BufferData(
			BufferTargetARB.ArrayBuffer,
			(nuint)(MaxQuads * QuadVerticesCount * Unsafe.SizeOf<SpriteVertex>()),
			null,
			BufferUsageARB.DynamicDraw);

		uint stride = (uint)Unsafe.SizeOf<SpriteVertex>();
		nint offset = 0;

		// --- TexId (int) ---
		GL.EnableVertexAttribArray(0);
		GL.VertexAttribIPointer(0, 1, VertexAttribIType.Int, stride, offset);
		offset += sizeof(int);

		// --- GType (int) ---
		GL.EnableVertexAttribArray(1);
		GL.VertexAttribIPointer(1, 1, VertexAttribIType.Int, stride, offset);
		offset += sizeof(int);

		// --- Position (Vector2) ---
		GL.EnableVertexAttribArray(2);
		GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, offset);
		offset += Unsafe.SizeOf<Vector2f>();

		// --- TexCoord (Vector2) ---
		GL.EnableVertexAttribArray(3);
		GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, stride, offset);
		offset += Unsafe.SizeOf<Vector2f>();

		// --- Color (Vector4) ---
		GL.EnableVertexAttribArray(4);
		GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, stride, offset);
		offset += Unsafe.SizeOf<Vector4f>();

		GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
		GL.BindVertexArray(0);
	}

	public void AddSprite(
		Matrix4f model,
		Texture2d texture,
		GeometryType geometryType = GeometryType.Quad,
		Rect? textureRegion = null,
		Vector4f? color = null,
		Vector2f? pivot = null)
	{
		if (_verticesUsed + 4 >= (uint)_vertices.Length)
		{
			Flush();
		}

		bool foundTexture = false;
		int textureIndex = -1;
		for (int i = 0; i < _textureUsed; i++)
		{
			if (_textures[i].Handle == texture.Handle)
			{
				foundTexture = true;
				textureIndex = i;
				break;
			}
		}

		if (!foundTexture && _textureUsed >= _textures.Length)
		{
			Flush();
		}

		if (!foundTexture)
		{
			_textures[_textureUsed] = texture;
			textureIndex = _textureUsed;
			_textureUsed++;
		}

		textureRegion ??= new Rect(0f, 0f, texture.Width, texture.Height);
		color ??= Vector4f.One;

		var normalizedRegion = new Rect(
			textureRegion.Value.X / texture.Width,
			(texture.Height - textureRegion.Value.Y) / texture.Height,
			textureRegion.Value.Width / texture.Width,
			textureRegion.Value.Height / texture.Height
		);

		Span<Vector2f> texCoord =
		[
			new (normalizedRegion.X, normalizedRegion.Y - normalizedRegion.Height),
			new (normalizedRegion.X + normalizedRegion.Width, normalizedRegion.Y - normalizedRegion.Height),
			new (normalizedRegion.X + normalizedRegion.Width, normalizedRegion.Y),
			new (normalizedRegion.X, normalizedRegion.Y),
		];

		var baseSpriteVertex = new SpriteVertex(
			textureIndex,
			(int)geometryType,
			Vector2f.One,
			Vector2f.One,
			color.Value);

		var pivotVal = pivot ?? new Vector2f(0.5f, 0.5f);
		for (int i = 0; i < 4; i++)
		{
			var localPos = new Vector4f(
				QuadVertices[i].X - pivotVal.X,
				QuadVertices[i].Y - pivotVal.Y,
				QuadVertices[i].Z,
				QuadVertices[i].W);

			Vector2f finalPos = (localPos * model).Xy();
			_vertices[_verticesUsed + i] = baseSpriteVertex with
			{
				Position = finalPos,
				TexCoord = texCoord[i],
			};
		}

		_verticesUsed += 4;
	}

	public unsafe void Flush()
	{
		if (_verticesUsed == 0)
			return;

		GL.BindVertexArray(_vao);
		GL.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

		// For Silk.NET, BufferSubData often uses unsafe code
		unsafe
		{
			fixed (SpriteVertex* verticesPtr = _vertices)
			{
				GL.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(_verticesUsed * sizeof(SpriteVertex)), verticesPtr);
			}
		}

		// Bind textures
		for (int i = 0; i < _textureUsed; i++)
		{
			_textures[i].Bind((uint)i);
		}

		uint quadsUsed = _verticesUsed / 4;
		uint totalIndices = quadsUsed * 6;

		GL.DrawElements(
			PrimitiveType.Triangles,
			totalIndices,
			DrawElementsType.UnsignedInt,
			(void*)0
		);

		_verticesUsed = 0;
		_textureUsed = 0;
		GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
		GL.BindVertexArray(0);
	}

	public void Dispose()
	{
		GL.DeleteVertexArray(_vao);
		GL.DeleteBuffer(_vbo);
		GL.DeleteBuffer(_ebo);
	}
}