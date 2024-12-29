using OpenTK.Mathematics;
using Step.Main.Editor;
using System.Text.Json.Serialization;

namespace Step.Main.Graphics.Particles;

public class Emitter
{
	[EditorProperty(1, 2000)]
	public int Amount { get; set; }

	[EditorProperty(1f, 100f)]
	public Vector2 Size { get; set; } = Vector2.One;

	[EditorProperty(0f, 20f)]
	public float Lifetime { get; set; }

	[EditorProperty(0f, 1f)]
	public float Explosiveness { get; set; }

	[EditorProperty()]
	public bool OneShot { get; set; }

	[EditorProperty(0f, 300f)]
	public float MinSpeed { get; set; }

	[EditorProperty(0f, 300f)]
	public float MaxSpeed { get; set; }

	[EditorProperty(0f, 6.28f)]
	public float DirectionAngle { get; set; }

	[JsonIgnore]
	public Vector2 DirectionSign { get; set; } = Vector2.One;

	[EditorProperty(0f, 3.14f)]
	public float Spread { get; set; }

	[EditorProperty()]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public ParticlesMaterial? Material { get; set; } = ParticlesMaterial.Default;
}

public class ParticlesMaterial
{
	[EditorProperty(isColor: true)]
	public Vector4 ColorMin { get; set; }

	[EditorProperty(isColor: true)]
	public Vector4 ColorMax { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public Texture2d? Texture { get; set; }

	public static readonly ParticlesMaterial Default = new()
	{
		ColorMin = Vector4.One,
		ColorMax = Vector4.One,
		Texture = null
	};
}