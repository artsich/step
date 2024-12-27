using OpenTK.Mathematics;
using Step.Main.Graphics;

namespace Step.Main.Gameplay.Spawn;

public sealed class SpawnSpeedEntity(Texture2d texture) : SpawnEntity(
		0.5f,
		(gs) => gs.EffectsCount<SpeedEffect>() < 2,
		(pos, gs) => new Thing(pos, new Vector2(20), new SpeedEffect(gs.Player))
		{
			Texture = texture
		}
	)
{
}
