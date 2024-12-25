using OpenTK.Mathematics;

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

public class Particles
{
	private readonly Random _rand = new();

	private readonly Emitter _emitter;

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

	public Particles(Emitter emitter)
	{
		_emitter = emitter;
		_particles = new Particle[_emitter.Amount];
	}

	public void Update(float deltaTime)
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

			if (_particles[i].Age > _emitter.Lifetime)
			{
				if (_emitter.OneShot)
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

	public Span<Particle> GetParticles()
	{
		var last = Math.Max(0, _activeParticles);
		return _particles.AsSpan()[0..last];
	}

	private void Restart()
	{
		if (_particles == null || _particles.Length != _emitter.Amount)
		{
			_particles = new Particle[_emitter.Amount];
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
		float startT = _emitter.Lifetime * (1f - _emitter.Explosiveness) * ratio;
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
		float offset = ((float)_rand.NextDouble() * 2f - 1f) * _emitter.Spread;
		float angle = _emitter.DirectionAngle + offset;
		float speed = _emitter.MinSpeed + (float)_rand.NextDouble() * (_emitter.MaxSpeed - _emitter.MinSpeed);

		return new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
	}
}
