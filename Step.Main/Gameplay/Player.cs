using Step.Engine;

namespace Step.Main.Gameplay;

public class Player(Input input) : GameObject
{
	public float Speed { get; set; } = 30f;

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate(float deltaTime)
	{
		var pos = LocalTransform.Position;
		var mouse = input.MouseScreenPosition;

		var dir = (mouse - pos).Normalized();

		pos += dir * Speed * deltaTime;
		LocalTransform.Position = pos;
	}
}
