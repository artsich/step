using OpenTK.Mathematics;

namespace Step.Main;

public class HealthThing(Vector2 position, Vector2 size, float acceleration = 20f) 
	: Thing(position, size, acceleration)
{
	public int Hp { get; init; } = 1;

	public override Color4<Rgba> Color => Color4.Red;

	public override void ApplyEffect(Player player)
	{
		player.AddHp(Hp);
	}
}
