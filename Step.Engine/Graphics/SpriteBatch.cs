using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;

namespace Step.Engine.Graphics;

public sealed class SpriteBatch : IDisposable
{
	private record struct SpriteVertex(
		int TexId,
		Vector2 Position,
		Vector2 TexCoord,
		Vector4 Color);

	private const int QuadVerticesCount = 4;
	
	private const int MaxQuads = 100_000;
	private const int IndicesPerQuad = 6;
	private uint[] _indices;

	private int _verticesUsed;
	private readonly SpriteVertex[] _vertices;
	private int _textureUsed;
	private readonly Texture2d[] _textures;

	private readonly Vector4[] QuadVertices =
	{
		new(0f, 0f, 0f, 1f),
		new(1f, 0f, 0f, 1f),
		new(1f, 1f, 0f, 1f),
		new(0f, 1f, 0f, 1f)
	};

	private int _vao, _vbo, _ebo;

	public readonly int MaxTextures;

	public SpriteBatch()
	{
		GL.GetInteger(GetPName.MaxTextureImageUnits, out MaxTextures);
		_textures = new Texture2d[MaxTextures];
		_vertices = new SpriteVertex[MaxQuads * QuadVerticesCount];

		InitializeBuffers();
	}

	private void InitializeBuffers()
	{
		_indices = new uint[MaxQuads * IndicesPerQuad];

		int uboOffset = 0;
		for (int i = 0; i < MaxQuads; i++)
		{
			_indices[i * 6 + 0] = (uint)(uboOffset + 0);
			_indices[i * 6 + 1] = (uint)(uboOffset + 1);
			_indices[i * 6 + 2] = (uint)(uboOffset + 2);
			_indices[i * 6 + 3] = (uint)(uboOffset + 2);
			_indices[i * 6 + 4] = (uint)(uboOffset + 3);
			_indices[i * 6 + 5] = (uint)(uboOffset + 0);

			uboOffset += 4;
		}

		_vao = GL.GenVertexArray();
		GL.BindVertexArray(_vao);

		_ebo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
		GL.BufferData(
			BufferTarget.ElementArrayBuffer,
			_indices.Length * sizeof(uint),
			_indices,
			BufferUsage.StaticDraw
		);

		_vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
		GL.BufferData(BufferTarget.ArrayBuffer,
			_vertices.Length * Unsafe.SizeOf<SpriteVertex>(),
			IntPtr.Zero,
			BufferUsage.DynamicDraw);

		int stride = Unsafe.SizeOf<SpriteVertex>();
		int offset = 0;

		// --- TexId (int) ---
		GL.EnableVertexAttribArray(0);
		GL.VertexAttribIPointer(0, 1, VertexAttribIType.Int, stride, offset);
		offset += sizeof(int);

		// --- Position (Vector2) ---
		GL.EnableVertexAttribArray(1);
		GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, offset);
		offset += Vector2.SizeInBytes;

		// --- TexCoord (Vector2) ---
		GL.EnableVertexAttribArray(2);
		GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, offset);
		offset += Vector2.SizeInBytes;

		// --- Color (Vector4) ---
		GL.EnableVertexAttribArray(3);
		GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, stride, offset);
		offset += Vector4.SizeInBytes;

		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindVertexArray(0);
	}

	public void AddSprite(
		Matrix4 model,
		Texture2d texture,
		Rect? textureRegion = null,
		Vector4? color = null,
		Vector2? pivot = null)
	{
		if (_verticesUsed + 4 >= _vertices.Length)
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

		textureRegion ??= new Rect(Vector2.Zero, new Vector2(texture.Width, texture.Height));
		color ??= Vector4.One;

		var normalizedRegion = new Rect(
			textureRegion.Value.Min / new Vector2(texture.Width, texture.Height),
			textureRegion.Value.Max / new Vector2(texture.Width, texture.Height)
		);

		Span<Vector2> texCoord =
		[
			normalizedRegion.Min,
			new Vector2(normalizedRegion.Max.X, normalizedRegion.Min.Y),
			normalizedRegion.Max,
			new Vector2(normalizedRegion.Min.X, normalizedRegion.Max.Y),
		];

		var baseSpriteVertex = new SpriteVertex(
			textureIndex,
			Vector2.One,
			Vector2.One,
			color.Value);

		var pivotVal = pivot ?? new Vector2(0.5f, 0.5f);
		for (int i = 0; i < 4; i++)
		{
			var localPos = new Vector4(
				QuadVertices[i].X - pivotVal.X,
				QuadVertices[i].Y - pivotVal.Y,
				QuadVertices[i].Z,
				QuadVertices[i].W);

			Vector2 finalPos = (localPos * model).Xy;
			_vertices[_verticesUsed + i] = baseSpriteVertex with
			{
				Position = finalPos,
				TexCoord = texCoord[i],
			};
		}

		_verticesUsed += 4;
	}

	public void Flush()
	{
		if (_verticesUsed == 0)
			return;

		GL.BindVertexArray(_vao);
		GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

		int sizeInBytes = _verticesUsed * Unsafe.SizeOf<SpriteVertex>();
		GL.BufferSubData(
			BufferTarget.ArrayBuffer,
			IntPtr.Zero,
			sizeInBytes,
			_vertices);

		for (uint i = 0; i < _textureUsed; i++)
		{
			_textures[i].Bind(i);
		}

		var quadsUsed = _verticesUsed / 4;
		var totalIndices = quadsUsed * 6;
		GL.DrawElements(
			PrimitiveType.Triangles,
			totalIndices,
			DrawElementsType.UnsignedInt,
			IntPtr.Zero
		);

		_verticesUsed = 0;
		_textureUsed = 0;

		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindVertexArray(0);
	}

	public void Dispose()
	{
		GL.DeleteVertexArray(_vao);
	}
}
