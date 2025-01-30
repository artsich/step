using OpenTK.Mathematics;
using Step.Engine;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.Actors;

public class PlayerSprite() : GameObject(name: "Player Health Sprite")
{
	private Player _player;
	private Sprite2d _spriteHp;

	private float _initialHeightScale;

	protected override void OnStart()
	{
		_player = Parent as Player ?? throw new InvalidOperationException();
		_spriteHp = GetChildOf<Sprite2d>("Health");

		_initialHeightScale = _spriteHp.LocalTransform.Scale.Y;
	}

	protected override void OnUpdate(float deltaTime)
	{
		var hpScale = _initialHeightScale * (_player.Hp / _player.MaxHp);
		var spriteScale = _spriteHp.LocalTransform.Scale;
		_spriteHp.LocalTransform.Scale = new Vector2(spriteScale.X, hpScale);
	}
}
