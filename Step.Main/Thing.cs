using OpenTK.Mathematics;

namespace Step.Main;

public interface IThing
{
	void Update(float dt);

	void ApplyEffect(Player player);
}

public class Thing(Vector2 position, Vector2 size, float acceleration = 20f)
{
	public Vector2 Position { get; private set; } = position;

	public Vector2 Size { get; } = size;

	public void Update(float dt)
	{
		var pos = Position;
		pos.Y -= acceleration * dt;
		Position = pos;
	}

	public Box2 BoundingBox => new(Position - (Size / 2f), Position + (Size / 2f));
}