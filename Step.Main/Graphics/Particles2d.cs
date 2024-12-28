using OpenTK.Mathematics;
using Step.Main.Gameplay;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Step.Main.Graphics;

public struct Particle
{
	public bool Active;
	public float StartTime;
	public float Age;
	public Vector2 Position;
	public Vector2 Velocity;
}

public class Emitter
{
	public int Amount { get; set; }
	public Vector2 Size { get; set; } = Vector2.One;
	public float Lifetime { get; set; }
	public float Explosiveness { get; set; }
	public bool OneShot { get; set; }
	public float MinSpeed { get; set; }
	public float MaxSpeed { get; set; }
	public float DirectionAngle { get; set; }
	public float Spread { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public ParticlesMaterial? Material { get; set; } = ParticlesMaterial.Default;
}

public class ParticlesMaterial
{
	public Vector4 ColorMin { get; set; }

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

public class Vector2JsonConverter : JsonConverter<Vector2>
{
	public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var array = JsonSerializer.Deserialize<float[]>(ref reader, options);
		return new Vector2(array[0], array[1]);
	}

	public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		writer.WriteNumberValue(value.X);
		writer.WriteNumberValue(value.Y);
		writer.WriteEndArray();
	}
}

public class Vector4JsonConverter : JsonConverter<Vector4>
{
	public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var array = JsonSerializer.Deserialize<float[]>(ref reader, options);
		return new Vector4(array[0], array[1], array[2], array[3]);
	}

	public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		writer.WriteNumberValue(value.X);
		writer.WriteNumberValue(value.Y);
		writer.WriteNumberValue(value.Z);
		writer.WriteNumberValue(value.W);
		writer.WriteEndArray();
	}
}

public class Particles2d : GameObject
{
	private readonly Random _rand = new();

	private readonly Renderer _renderer;

	private int _activeParticles;
	private Particle[] _particles;

	public Emitter Emitter { get; private set; }

	private bool _emitting;
	public bool Emitting
	{
		get => _emitting;
		set
		{
			_emitting = value;
			if (_emitting)
			{
				Restart();
			}
		}
	}

	public Particles2d(Emitter emitter, Renderer renderer)
	{
		Emitter = emitter;
		_particles = new Particle[Emitter.Amount];
		_renderer = renderer;
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (!_emitting)
			return;

		for (int i = 0; i < _activeParticles; i++)
		{
			if (!_particles[i].Active)
			{
				continue;
			}

			_particles[i].StartTime -= deltaTime;
			if (_particles[i].StartTime > 0)
			{
				continue;
			}

			if (_particles[i].Age > Emitter.Lifetime)
			{
				if (Emitter.OneShot)
				{
					_particles[i].Active = false;
					_particles[i] = _particles[_activeParticles - 1];
					_activeParticles--;
					continue;
				}
				else
				{
					_particles[i] = GenerateParticle(i);
				}
			}

			_particles[i].Age += deltaTime;
			_particles[i].Position += _particles[i].Velocity * deltaTime;
		}
	}

	protected override void OnRender()
	{
		Matrix4 globalMatrix = GetGlobalMatrix();

		var colorMin = Emitter.Material?.ColorMin ?? Vector4.One;
		var colorMax = Emitter.Material?.ColorMax ?? Vector4.One;

		var particles = GetParticles();
		for (int i = 0; i < particles.Length; i++)
		{
			var p = particles[i];
			if (!p.Active)
				continue;

			var color = Vector4.Lerp(colorMin, colorMax, p.Age / Emitter.Lifetime);
			var finalPos = new Vector4(p.Position, 0f, 1f) * globalMatrix;
			_renderer.DrawRect(
				finalPos.Xy,
				Emitter.Size,
				(Color4<Rgba>)color,
				Emitter.Material?.Texture);
		}
	}

	public Span<Particle> GetParticles()
	{
		var last = Math.Max(0, _activeParticles);
		return _particles.AsSpan()[0..last];
	}

	private void Restart()
	{
		if (_particles == null || _particles.Length != Emitter.Amount)
		{
			_particles = new Particle[Emitter.Amount];
		}

		_activeParticles = _particles.Length;

		for (int i = 0; i < _particles.Length; i++)
		{
			_particles[i] = GenerateParticle(i);
		}
	}

	private Particle GenerateParticle(int i)
	{
		float ratio = (float)i / Math.Max(1, _particles.Length - 1);
		float startT = Emitter.Lifetime * (1f - Emitter.Explosiveness) * ratio;
		return new Particle()
		{
			Active = true,
			StartTime = startT,
			Age = 0f,
			Position = Vector2.Zero,
			Velocity = RandomVelocity(),
		};
	}

	private Vector2 RandomVelocity()
	{
		float offset = ((float)_rand.NextDouble() * 2f - 1f) * Emitter.Spread;
		float angle = Emitter.DirectionAngle + offset;
		float speed = Emitter.MinSpeed + (float)_rand.NextDouble() * (Emitter.MaxSpeed - Emitter.MinSpeed);

		return new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
	}
}
