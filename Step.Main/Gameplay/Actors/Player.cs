using OpenTK.Mathematics;
using Step.Engine.Audio;
using Step.Engine.Collisions;
using Step.Engine.Editor;
using Step.Engine.Physics;

namespace Step.Main.Gameplay.Actors;

public class Player : KinematicBody2D, ITarget
{
	[EditorProperty]
	public float Speed { get; set; } = 30f;

	public float MaxHp { get; private set; } = 5f;

	[EditorProperty]
	public float Hp { get; private set; } = 5f;

	public event Action? OnDeath;

	public event Action? OnDamage;

	public Vector2 Position => GlobalPosition;

	private readonly PlayerAbilities _playerAbilities;

	private CollisionShape? _collisionShape;
	private readonly Input _input;

	public Player(Input input, CollisionShape collisionShape)
		: base(collisionShape)
	{
		_playerAbilities = new(input, this);
		_input = input;

		Name = nameof(Player);
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

	private void OnCollision(CollisionShape shape, CollisionInfo _)
	{
		if (shape.Parent is GliderEntity glider)
		{
			AudioManager.Ins.PlaySound("player_hurt_glider");
			TakeDamage(1);
			glider.QueueFree();
		}
		else if (shape.Parent is CircleEnemy circle)
		{
			AudioManager.Ins.PlaySound("player_hurt_circle");
			TakeDamage(1);
			circle.QueueFree();
		}
		else if (shape.Parent is CrossEnemy cross)
		{
			AudioManager.Ins.PlaySound("player_pickup");
			cross.QueueFree();
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
			var dir = diff.Normalized();
			Velocity = dir * Speed;
		}
		else
		{
			Velocity = Vector2.Zero;
		}
	}

	protected override void OnDebugDraw()
	{
		EditOf.Render(this);
	}
}
