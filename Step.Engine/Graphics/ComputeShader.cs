using Silk.NET.OpenGL;

namespace Step.Engine.Graphics;

public sealed unsafe class ComputeShader : IDisposable
{
	public readonly uint Handle;
	private readonly Dictionary<string, int> _uniformLocations = [];
	private static GL GL => Ctx.GL;

	private ComputeShader(uint handle)
	{
		Handle = handle;
	}

	public static ComputeShader FromFile(string path)
	{
		return FromSource(File.ReadAllText(path));
	}

	public static ComputeShader FromSource(string source)
	{
		var computeShader = GL.CreateShader(ShaderType.ComputeShader);
		GL.ShaderSource(computeShader, source);

		CompileShader(computeShader);

		var handle = GL.CreateProgram();
		GL.AttachShader(handle, computeShader);

		LinkProgram(handle);

		GL.DetachShader(handle, computeShader);
		GL.DeleteShader(computeShader);

		GL.GetProgram(handle, ProgramPropertyARB.ActiveUniforms, out var numberOfUniforms);

		return new ComputeShader(handle);
	}

	private static void CompileShader(uint shader)
	{
		GL.CompileShader(shader);

		GL.GetShader(shader, ShaderParameterName.CompileStatus, out var code);
		if (code == (int)GLEnum.True)
			return;

		GL.GetShaderInfoLog(shader, out var infoLog);
		if (!string.IsNullOrEmpty(infoLog))
		{
			throw new Exception($"Error occurred whilst compiling Compute Shader({shader}).\n\n{infoLog}");
		}
	}

	private static void LinkProgram(uint program)
	{
		GL.LinkProgram(program);

		GL.GetProgram(program, ProgramPropertyARB.LinkStatus, out var code);
		if (code != (int)GLEnum.True)
		{
			var infoLog = GL.GetProgramInfoLog(program);
			throw new Exception($"Error occurred whilst linking Compute Program({program}): {infoLog}");
		}
	}

	public void Use()
	{
		GL.UseProgram(Handle);
	}

	private int GetUniformLocation(string name)
	{
		if (!_uniformLocations.TryGetValue(name, out var index))
		{
			_uniformLocations[name] = index = GL.GetUniformLocation(Handle, name);
		}

		return index;
	}

	public void SetInt(string name, int data)
	{
		GL.UseProgram(Handle);
		GL.Uniform1(GetUniformLocation(name), data);
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
		GL.Uniform4(GetUniformLocation(name), v.X, v.Y, v.Z, v.W);
	}

	public unsafe void Set(string name, Span<int> values)
	{
		GL.ProgramUniform1(
			Handle,
			GetUniformLocation(name),
			values);
	}

	public void Dispatch(uint numGroupsX, uint numGroupsY, uint numGroupsZ)
	{
		GL.UseProgram(Handle);
		GL.DispatchCompute(numGroupsX, numGroupsY, numGroupsZ);
	}

	public void MemoryBarrier(MemoryBarrierMask barriers)
	{
		GL.MemoryBarrier(barriers);
	}

	public void Dispose()
	{
		GL.DeleteProgram(Handle);
	}
}