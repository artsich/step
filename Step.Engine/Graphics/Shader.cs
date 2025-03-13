using Silk.NET.OpenGL;

namespace Step.Engine.Graphics;

public sealed unsafe class Shader : IDisposable
{
	public readonly uint Handle;

	private readonly Dictionary<string, int> _uniformLocations = [];
	private readonly GL GL;

	public Shader(string vertPath, string fragPath)
	{
		GL = Ctx.GL;
		var vertexShader = LoadShader(vertPath, ShaderType.VertexShader);
		var fragmentShader = LoadShader(fragPath, ShaderType.FragmentShader);

		Handle = GL.CreateProgram();
		GL.AttachShader(Handle, vertexShader);
		GL.AttachShader(Handle, fragmentShader);

		LinkProgram(Handle);

		GL.DetachShader(Handle, vertexShader);
		GL.DetachShader(Handle, fragmentShader);
		GL.DeleteShader(fragmentShader);
		GL.DeleteShader(vertexShader);

		GL.GetProgram(Handle, ProgramPropertyARB.ActiveUniforms, out var numberOfUniforms);
	}

	private uint LoadShader(string vertPath, ShaderType type)
	{
		var shaderSource = File.ReadAllText(vertPath);
		var vertexShader = GL.CreateShader(type);
		GL.ShaderSource(vertexShader, shaderSource);

		CompileShader(vertexShader);
		return vertexShader;
	}

	private void CompileShader(uint shader)
	{
		GL.CompileShader(shader);

		GL.GetShader(shader, ShaderParameterName.CompileStatus, out var code);
		if (code == (int)GLEnum.True)
			return;

		GL.GetShaderInfoLog(shader, out var infoLog);
		if (!string.IsNullOrEmpty(infoLog))
		{
			throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
		}
	}

	private void LinkProgram(uint program)
	{
		GL.LinkProgram(program);

		GL.GetProgram(program, ProgramPropertyARB.LinkStatus, out var code);
		if (code != (int)GLEnum.True)
		{
			var infoLog = GL.GetProgramInfoLog(program);
			throw new Exception($"Error occurred whilst linking Program({program}): {infoLog}");
		}
	}

	public void Use()
	{
		GL.UseProgram(Handle);
	}

	public int GetAttribLocation(string attribName)
	{
		return GL.GetAttribLocation(Handle, attribName);
	}

	public void SetInt(string name, int data)
	{
		GL.UseProgram(Handle);
		GL.Uniform1(GetUniformLocation(name), data);
	}

	private int GetUniformLocation(string name)
	{
		if (!_uniformLocations.TryGetValue(name, out var index))
		{
			_uniformLocations[name] = index = GL.GetUniformLocation(Handle, name);
		}

		return index;
	}

	public void SetFloat(string name, float data)
	{
		GL.UseProgram(Handle);
		GL.Uniform1(GetUniformLocation(name), data);
	}

	public void SetMatrix4(string name, Matrix4f data, bool transpose = false)
	{
		GL.UseProgram(Handle);
		GL.UniformMatrix4(GetUniformLocation(name), 1, transpose, (float*)&data);
	}

	public void SetVector2(string name, Vector2f v)
	{
		GL.UseProgram(Handle);
		GL.Uniform2(GetUniformLocation(name), v.X, v.Y);
	}

	public void SetVector3(string name, Vector3f v)
	{
		GL.UseProgram(Handle);
		GL.Uniform3(GetUniformLocation(name), v.X, v.Y, v.Z);
	}

	public void SetVector4(string name, Vector4f v)
	{
		GL.UseProgram(Handle);
		GL.Uniform4(GetUniformLocation(name), v.X, v.Y,	v.Z, v.W);
	}

	public unsafe void Set(string name, Span<int> values)
	{
		GL.ProgramUniform1(
			Handle,
			GetUniformLocation(name),
			values);
	}

	public void Dispose()
	{
		GL.DeleteProgram(Handle);
	}
}
