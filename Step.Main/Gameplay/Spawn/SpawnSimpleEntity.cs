using OpenTK.Mathematics;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.Spawn;

public sealed class SpawnSimpleEntity(Texture2d texture, Renderer renderer, bool isFriend) : SpawnEntity(
		0.9f,
		(gs) => true,
		(pos, gs) => new Thing(pos, new Vector2(20, 20), renderer)
		{
			Texture = texture,
			IsFriend = isFriend,
			Color = isFriend ? Color4.Lightgreen : Color4.Orangered,
		}
	)
{
}
