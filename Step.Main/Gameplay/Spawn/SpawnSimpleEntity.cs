using OpenTK.Mathematics;
using Step.Main.Graphics;

namespace Step.Main.Gameplay.Spawn;

public sealed class SpawnSimpleEntity(Texture2d texture, Renderer renderer) : SpawnEntity(
		0.9f,
		(gs) => true,
		(pos, gs) => new Thing(pos, new Vector2(20, 20), renderer)
		{
			Texture = texture
		}
	)
{
}
