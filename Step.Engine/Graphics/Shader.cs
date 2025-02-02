using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Serilog;

namespace Step.Engine.Graphics;

public class Shader
{
	public readonly int Handle;

	private readonly Dictionary<string, int> _uniformLocations;

	public Shader(string vertPath, string fragPath)
	{
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

		GL.GetProgrami(Handle, ProgramProperty.ActiveUniforms, out var numberOfUniforms);

		_uniformLocations = new Dictionary<string, int>();

		for (uint i = 0; i < numberOfUniforms; i++)
		{
			var key = GL.GetActiveUniformName(Handle, i, 50, out _);
			var location = GL.GetUniformLocation(Handle, key);
			_uniformLocations.Add(key, location);
		}
	}

	private static int LoadShader(string vertPath, ShaderType type)
	{
		var shaderSource = File.ReadAllText(vertPath);
		var vertexShader = GL.CreateShader(type);
		GL.ShaderSource(vertexShader, shaderSource);

		CompileShader(vertexShader);
		return vertexShader;
	}

	private static void CompileShader(int shader)
	{
		GL.CompileShader(shader);

		GL.GetShaderi(shader, ShaderParameterName.CompileStatus, out var code);
		if (code == (int)All.True)
			return;

		GL.GetShaderInfoLog(shader, out var infoLog);
		if (!string.IsNullOrEmpty(infoLog))
		{
			throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
		}
	}

	private static void LinkProgram(int program)
	{
		GL.LinkProgram(program);

		GL.GetProgrami(program, ProgramProperty.LinkStatus, out var code);
		if (code != (int)All.True)
		{
			throw new Exception($"Error occurred whilst linking Program({program})");
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
		GL.Uniform1i(GetUniformLocation(name), data);
	}

	private int GetUniformLocation(string name)
	{
		if (_uniformLocations.TryGetValue(name, out var index))
		{
			return index;
		}
		else
		{
			return -1;
		}
	}

	public void SetFloat(string name, float data)
	{
		GL.UseProgram(Handle);
		GL.Uniform1f(GetUniformLocation(name), data);
	}

	public void SetMatrix4(string name, Matrix4 data, bool transpose = false)
	{
		GL.UseProgram(Handle);
		GL.UniformMatrix4f(GetUniformLocation(name), 1, transpose, ref data);
	}

	public void SetVector2(string name, Vector2 data)
	{
		GL.UseProgram(Handle);
		GL.Uniform2f(GetUniformLocation(name), 1, ref data);
	}

	public void SetVector3(string name, Vector3 data)
	{
		GL.UseProgram(Handle);
		GL.Uniform3f(GetUniformLocation(name), 1, ref data);
	}

	public void SetVector4(string name, Vector4 data)
	{
		GL.UseProgram(Handle);
		GL.Uniform4f(GetUniformLocation(name), 1, ref data);
	}

	public void SetColor(string name, Color4<Rgba> data)
	{
		Vector4 color = new(data.X, data.Y, data.Z, data.W);
		SetVector4(name, color);
	}

	public unsafe void Set(string name, Span<int> values)
	{
		fixed (int* ptr = values)
		{
			GL.ProgramUniform1iv(
				Handle,
				GetUniformLocation(name),
				values.Length,
				ptr);
		}
	}
}
