using Step.Engine.Collisions;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay;

public class Borderline(Renderer renderer) : RectangleShape2d(renderer)
{
	public Player Player { private get; init; }

	public Camera2d Camera { private get; init; }

	protected override void OnStart()
	{
		OnCollision += OnCollisionWithEnemy;
		base.OnStart();
	}

	protected override void OnEnd()
	{
		OnCollision -= OnCollisionWithEnemy;
		base.OnEnd();
	}

	private void OnCollisionWithEnemy(CollisionShape shape)
	{
		if (shape is Thing thing)
		{
			if (!thing.IsFriend)
			{
				Player.Damage(1);
				Camera.Shake(5f, 0.5f);
			}

			shape.QueueFree();
		}
	}
}
