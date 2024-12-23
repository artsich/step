using OpenTK.Mathematics;

namespace Step.Main.Spawn;

public sealed class SpanwHealthEntity(Texture2d texture) : SpawnEntity(
		0.2f,
		(gs) => !gs.Player.IsFullHp && gs.EffectsCount<HealEffect>() < gs.Player.MaxHp,
		(pos, gs) => new Thing(pos, new Vector2(20, 20), new HealEffect(1, gs.Player))
		{
			Texture = texture
		}
	)
{
}
