using OpenTK.Mathematics;

namespace Step.Engine.Graphics.Particles;

public class Particles2d : CanvasItem
{
	struct Particle
	{
		public bool Active;
		public float StartTime;
		public float Age;
		public Vector2 Position;
		public Vector2 Velocity;
	}

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
		: base(nameof(Particles2d))
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
			_particles[i].Position += (_particles[i].Velocity * Emitter.DirectionSign) * deltaTime;
		}

		_emitting = _activeParticles > 0;
	}

	protected override void OnRender()
	{
		if (!Visible)
		{
			return;
		}

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
				(Color4<Rgba>) (color * (Vector4)Color),
				Emitter.Material?.Texture,
				layer: Layer);
		}
	}

	private Span<Particle> GetParticles()
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
