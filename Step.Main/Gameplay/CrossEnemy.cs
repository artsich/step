using Step.Engine;

namespace Step.Main.Gameplay;

public class CrossEnemy()
	: GameObject(name: nameof(CrossEnemy))
{
	private GameObject? _target;

	public float Speed { get; set; } = 20f;

	public void Follow(GameObject target)
	{
		_target = target;
	}

	public void Unfollow()
	{
		_target = null;
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (_target != null)
		{
			var direction = (_target.GlobalPosition - GlobalPosition).Normalized();
			LocalTransform.Position += direction * Speed * deltaTime;
		}
	}
}
