using OpenTK.Mathematics;
using Step.Main.Graphics;

namespace Step.Main.Gameplay.Spawn;

public sealed class SpawnKillAllEntity(Texture2d texture) : SpawnEntity(
	0.1f,
	(gs) => gs.EffectsCount<KillAllEffect>() < 1,
	(pos, gs) => new Thing(pos, new Vector2(20, 20), new KillAllEffect(gs))
	{
		Texture = texture
	}
)
{
}
