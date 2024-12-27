using OpenTK.Mathematics;
using Step.Main.Gameplay;

namespace Step.Main;

public class Thing(Vector2 position, Vector2 size, IEffect? effect = null)
{
	public float Speed { get; set; } = 60f;

	public Vector2 Position { get; private set; } = position;

	public Vector2 Size { get; } = size;

	public Color4<Rgba> Color { get; init; } = Color4.Green;

	public Texture2d? Texture { get; init; }

	public void Update(float dt)
	{
		var pos = Position;
		pos.Y -= Speed * dt;
		Position = pos;
	}

	public void ApplyEffect(Player player)
	{
		if (effect is not null)
		{
			player.AddEffect(effect);
		}
	}

	public bool HasEffect<T>() where T : IEffect => effect is T;

	public Box2 BoundingBox => new(Position - (Size / 2f), Position + (Size / 2f));
}