using Silk.NET.Maths;
using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Collisions;
using Step.Engine.Editor;
using Step.Engine.Graphics.Particles;
using Step.Engine.Physics;

namespace Step.Main.Gameplay.Actors;

public class Player : KinematicBody2D, ITarget
{
	[EditorProperty]
	public float Speed { get; set; } = 30f;

	public float MaxHp { get; private set; } = 10f;

	[EditorProperty]
	public float Hp { get; private set; }

	public event Action? OnDeath;

	public event Action? OnDamage;

	public event Action? OnCrossCoinCollected;

	public Vector2f Position => GlobalPosition;

	private readonly PlayerAbilities _playerAbilities;

	private CollisionShape? _collisionShape;
	private Particles2d _collisionWithEnemyParticles;
	private readonly Input _input;

	public Player(Input input, CollisionShape collisionShape)
		: base(collisionShape, nameof(Player))
	{
		_playerAbilities = new(input, this);
		_input = input;
		Hp = MaxHp;
	}

	public void Heal(float healFactor)
	{
		Hp = Math.Min(Hp + healFactor, MaxHp);
	}

	public void AddAbility(IAbility ability) => _playerAbilities.Add(ability);

	protected override void OnStart()
	{
		base.OnStart();
		_collisionShape = GetChildOf<CollisionShape>();
		_collisionShape.OnCollision += OnCollision;
		_collisionWithEnemyParticles = GetChildOf<Particles2d>("ParticlesPlayerDamage");
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		_playerAbilities.Clear();
	}

	private void TakeDamage(float amount)
	{
		Hp -= amount;
		if (Hp <= 0)
		{
			OnDeath?.Invoke();
			QueueFree();
		}
		else
		{
			OnDamage?.Invoke();
		}
	}

	private void OnCollision(CollisionShape shape, CollisionInfo collisionInfo)
	{
		if (shape.Parent is GliderEntity glider)
		{
			GetChildOf<Sound>("player_hurt_glider").Play();
			TakeDamage(1);
			_collisionWithEnemyParticles.GlobalPosition = collisionInfo.Position;
			_collisionWithEnemyParticles.Emitting = true;
			glider.QueueFree();
		}
		else if (shape.Parent is CircleEnemy circle)
		{
			GetChildOf<Sound>("player_hurt_circle").Play();
			_collisionWithEnemyParticles.GlobalPosition = collisionInfo.Position;
			_collisionWithEnemyParticles.Emitting = true;
			TakeDamage(1);
			circle.QueueFree();
		}
		else if (shape.Parent is CrossEnemy cross)
		{
			GetChildOf<Sound>("player_pickup").Play();
			cross.QueueFree();
			OnCrossCoinCollected?.Invoke();
		}
	}

	protected override void OnUpdate(float deltaTime)
	{
		_playerAbilities.Update(deltaTime);
		Move();

		base.OnUpdate(deltaTime);
	}

	private void Move()
	{
		var mouse = _input.MouseWorldPosition;
		var diff = mouse - GlobalPosition;

		if (diff.LengthSquared > 1f)
		{
			var dir = Vector2D.Normalize(diff);
			Velocity = dir * Speed;
		}
		else
		{
			Velocity = Vector2f.Zero;
		}
	}

	protected override void OnDebugDraw()
	{
		EditOf.Render(this);
	}
}
