using Step.Engine;
using Step.Engine.Graphics;
using Step.Main.Gameplay.TowerDefense.Core;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class Base : GameObject
{
	private const float BaseSize = 40f;
	private const float MaxHealth = 100f;
	private const float DamagePerEnemy = 10f;

	private readonly HealthBar _healthBar;

	private Spawns? _spawns;
	private bool _dead = false;

	public event Action? Dead;

	public Base(Renderer renderer, Level level) : base(nameof(Base))
	{
		LocalTransform.Position = level.BasePosition;

		var baseSprite = new Sprite2d(renderer, Assets.LoadTexture2d("Textures/Custle.png"))
		{
			Layer = 6
		};
		baseSprite.LocalTransform.Scale = new Vector2f(BaseSize, BaseSize);
		AddChild(baseSprite);

		float healthBarWidth = BaseSize;
		float healthBarHeight = 6f;
		float healthBarOffsetY = BaseSize * 0.65f;

		_healthBar = new HealthBar(
			renderer, MaxHealth, healthBarWidth, healthBarHeight, new Vector2f(0f, healthBarOffsetY), 7);

		AddChild(_healthBar);
	}

	protected override void OnStart()
	{
		base.OnStart();
		BindSpawns();
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (_spawns == null)
			BindSpawns();
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		UnbindSpawns();
	}

	private void BindSpawns()
	{
		if (_spawns != null || Parent == null)
			return;

		foreach (var spawns in Parent.GetChildsOf<Spawns>())
		{
			_spawns = spawns;
			_spawns.EnemyReachedBase += HandleEnemyReachedBase;
			break;
		}
	}

	private void UnbindSpawns()
	{
		if (_spawns == null)
			return;

		_spawns.EnemyReachedBase -= HandleEnemyReachedBase;
		_spawns = null;
	}

	private void HandleEnemyReachedBase(Enemy enemy)
	{
		_healthBar.ApplyDamage(DamagePerEnemy);

		if (_healthBar.Hp <= 0f && !_dead)
		{
			_dead = true;
			Dead?.Invoke();
		}
	}
}
