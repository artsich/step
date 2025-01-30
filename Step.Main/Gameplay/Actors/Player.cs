using OpenTK.Mathematics;
using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Collisions;
using Step.Engine.Editor;
using Step.Main.Gameplay.Actors;

namespace Step.Main.Gameplay;

public class Player : GameObject, ITarget
{
	[EditorProperty]
	public float Speed { get; set; } = 30f;

	public float MaxHp { get; private set; } = 5f;

	[EditorProperty]
	public float Hp { get; private set; } = 5f;

	public event Action OnDeath;

	public event Action OnDamage;

	public Vector2 Position => GlobalPosition;

	private PlayerAbilities _playerAbilities;
	private RectangleShape2d _collisionShape;
	private readonly Input _input;

	public Player(Input input)
		: base(name: nameof(Player))
	{
		_playerAbilities = new(input, this);
		_input = input;
	}

	public void Heal(float healFactor)
	{
		Hp = Math.Min(Hp + healFactor, MaxHp);
	}

	public void AddAbility(IAbility ability) => _playerAbilities.Add(ability);

	protected override void OnStart()
	{
		_collisionShape = GetChildOf<RectangleShape2d>();
		_collisionShape.OnCollision += OnCollision;
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

	private void OnCollision(CollisionShape shape)
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
		Move(deltaTime);
	}

	private void Move(float deltaTime)
	{
		var pos = LocalTransform.Position;
		var mouse = _input.MouseScreenPosition;
		var diff = mouse - pos;

		if (diff.LengthSquared > 1f)
		{
			var dir = diff.Normalized();
			pos += dir * Speed * deltaTime;
			LocalTransform.Position = pos;
		}
	}

	protected override void OnDebugDraw()
	{
		EditOf.Render(this);
	}
}
