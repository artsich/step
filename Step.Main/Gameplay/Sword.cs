using OpenTK.Mathematics;
using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Collisions;
using Step.Engine.Editor;

namespace Step.Main.Gameplay;

public class Sword() : GameObject(nameof(Sword))
{
	[EditorProperty]
	public float InitialAngle { get; set; } = 2.45f;

	[EditorProperty]
	public float FinalAngle { get; set; } = -0.94f;

	[EditorProperty]
	public float Speed { get; set; } = 50f;

	private CollisionShape _hitbox;
	private bool _isAttack;

	protected override void OnStart()
	{
		_hitbox = GetChildOf<CollisionShape>("Sword hitbox");
		_hitbox.OnCollision += OnHitboxCollision;

		LocalTransform.Rotation = InitialAngle;
	}

	protected override void OnEnd()
	{
		_hitbox.OnCollision += OnHitboxCollision;
	}

	private void OnHitboxCollision(CollisionShape obj)
	{
		if (obj is Thing thing)
		{
			AudioManager.Ins.PlaySound("sword_hit");
			thing.QueueFree();
		}
	}

	public void Attack()
	{
		if (_isAttack)
		{
			return;
		}

		_isAttack = true;
		_hitbox.IsActive = true;
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (_isAttack)
		{
			LocalTransform.Rotation = MathHelper.Lerp(LocalTransform.Rotation, FinalAngle, Speed * deltaTime);

			if (LocalTransform.Rotation - FinalAngle < 0.1f)
			{
				_isAttack = false;
				_hitbox.IsActive = false;
				LocalTransform.Rotation = InitialAngle;
			}
		}
	}

	protected override void OnDebugDraw()
	{
		EditOf.Render(this);
	}
}
