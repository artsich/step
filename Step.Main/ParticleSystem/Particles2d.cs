using OpenTK.Mathematics;
using Step.Main.Gameplay;

namespace Step.Main.ParticleSystem;

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
	public int Amount;

	public float Lifetime;
	public float Explosiveness;

	public bool OneShot;

	public float MinSpeed;
	public float MaxSpeed;

	public float DirectionAngle;
	public float Spread;
}

public class Particles2d : GameObject
{
	private readonly Renderer _renderer;

	private readonly Random _rand = new();
	private readonly Texture2d _whiteTexture;

	public Emitter Emitter { get; private set; }

	private int _activeParticles;
	private Particle[] _particles;

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
		// TODO: Make default texture and bind to 0 slot...
		_whiteTexture = new Texture2d(".\\Assets\\Textures\\player.png").Load();
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

		var particles = GetParticles();
		for (int i = 0; i < particles.Length; i++)
		{
			var p = particles[i];
			if (!p.Active)
				continue;

			var colorMin = new Vector4(1f, 1f, 1f, 1f);
			var colorMax = new Vector4(1f, 1f, 1f, 0.3f);
			var color = Vector4.Lerp(colorMin, colorMax, p.Age / Emitter.Lifetime);

			var finalPos = new Vector4(p.Position, 0f, 1f) * globalMatrix;
			_renderer.DrawRect(finalPos.Xy, new(40f, 20f), (Color4<Rgba>)color, _whiteTexture);
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
