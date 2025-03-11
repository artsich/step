using Step.Engine.Editor;
using System.Text.Json.Serialization;

namespace Step.Engine.Graphics.Particles;

public class Emitter
{
	[EditorProperty(speed: 1, from: 0, to: 10000)]
	public int Amount { get; set; }

	[EditorProperty(1f, 100f)]
	public Vector2f Size { get; set; } = Vector2f.One;

	[EditorProperty(from: 0f, to: 100f, speed: 0.01f)]
	public float Lifetime { get; set; }

	[EditorProperty(0f, 1f, speed: 0.01f)]
	public float Explosiveness { get; set; }

	[EditorProperty()]
	public bool OneShot { get; set; }

	[EditorProperty(speed: 0.1f)]
	public float MinSpeed { get; set; }

	[EditorProperty(speed: 0.1f)]
	public float MaxSpeed { get; set; }

	[EditorProperty(-MathF.PI, MathF.PI, speed: 0.01f)]
	public float DirectionAngle { get; set; }

	[JsonIgnore]
	public Vector2f DirectionSign { get; set; } = Vector2f.One;

	[EditorProperty(0f, 3.14f, speed: 0.01f)]
	public float Spread { get; set; }

	[EditorProperty()]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public ParticlesMaterial? Material { get; set; } = ParticlesMaterial.Default;
}

public class ParticlesMaterial
{
	[EditorProperty(isColor: true)]
	public Vector4f ColorMin { get; set; }

	[EditorProperty(isColor: true)]
	public Vector4f ColorMax { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public Texture2d? Texture { get; set; }

	public static readonly ParticlesMaterial Default = new()
	{
		ColorMin = Vector4f.One,
		ColorMax = Vector4f.One,
		Texture = null
	};
}