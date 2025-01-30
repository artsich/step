using Step.Engine;

namespace Step.Main.Gameplay.Actors;

public class CrossEnemy()
	: GameObject(name: nameof(CrossEnemy))
{
	private GameObject? _target;

	public float Speed { get; set; } = 40f;

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
			GlobalPosition = GlobalPosition.MoveToward(_target.GlobalPosition, Speed * deltaTime);
		}
	}
}
