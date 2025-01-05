using OpenTK.Mathematics;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.Spawn;

public sealed class SpawnSpeedEntity(Texture2d texture, Renderer renderer) : SpawnEntity(
		0.5f,
		(gs) => gs.EffectsCount<SpeedEffect>() < 2,
		(pos, gs) => new Thing(pos, new Vector2(20), renderer, new SpeedEffect(gs.Player))
		{
			Texture = texture
		}
	)
{
}

public sealed class SpawnSizeChanger(Texture2d texture, Renderer renderer) : SpawnEntity(
	0.2f,
	(gs) => gs.EffectsCount<SizeChangeEffect>() < 1,
	(pos, gs) => new Thing(pos, new Vector2(20), renderer, new SizeChangeEffect(gs.Player))
	{
		Texture = texture
	})
{
}
