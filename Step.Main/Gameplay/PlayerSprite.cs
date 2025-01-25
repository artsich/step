using OpenTK.Mathematics;
using Step.Engine;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay;

public class PlayerSprite() : GameObject(name: "Player Health Sprite")
{
	private Player _player;
	private Sprite2d _spriteHp;

	private float _hpScale = 1f;

	protected override void OnStart()
	{
		_player = Parent as Player ?? throw new InvalidOperationException();
		_spriteHp = GetChildOf<Sprite2d>("Health");

		_player.OnDamage += OnPlayerDamage;

		_hpScale = _spriteHp.LocalTransform.Scale.Y / _player.MaxHp;
	}

	protected override void OnEnd()
	{
		_player.OnDamage -= OnPlayerDamage;
	}

	private void OnPlayerDamage()
	{
		var spriteScale = _spriteHp.LocalTransform.Scale;
		_spriteHp.LocalTransform.Scale = spriteScale - new Vector2(0f, _hpScale);
	}
}
