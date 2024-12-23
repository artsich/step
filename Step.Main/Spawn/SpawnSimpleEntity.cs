using OpenTK.Mathematics;

namespace Step.Main.Spawn;

public sealed class SpawnSimpleEntity(Texture2d texture) : SpawnEntity(
		0.9f,
		(gs) => true,
		(pos, gs) => new Thing(pos, new Vector2(20, 20))
		{
			Texture = texture
		}
	)
{
}
