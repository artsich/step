using OpenTK.Mathematics;
using Step.Engine;
using Step.Engine.Editor;

namespace Step.Main.Gameplay.Actors;

public class CircleEnemy(Vector2 targetDir)
	: GameObject(nameof(CircleEnemy))
{
	[EditorProperty]
	public float Speed { get; set; } = 30f;

	protected override void OnUpdate(float deltaTime)
	{
		GlobalPosition += targetDir * Speed * deltaTime;
	}
}
