using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Graphics;
using Step.Main.Gameplay.TowerDefense.Animations;
using Step.Main.Gameplay.TowerDefense.Core;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class Base : GameObject
{
	private const float BaseSize = 40f;
	private const float BaseMaxHealth = 100f;
	private const float DamagePerEnemy = 10f;
	private const int MaxFortificationLevel = 3;

	private readonly HealthBar _healthBar;

	private Spawns? _spawns;
	private bool _dead = false;
	private int _fortificationLevel = 0;

	public event Action? Dead;

	private const float HitScaleFactor = 1.2f;
	private const float HitDuration = 0.1f;

	private readonly Vector2f _baseScale;
	private readonly Sprite2d _baseSprite;
	private readonly SpriteHitAnimation _hitAnimation;

	public Base(Renderer renderer, Level level) : base(nameof(Base))
	{
		LocalTransform.Position = level.BasePosition;

		_baseSprite = new Sprite2d(renderer, Assets.LoadTexture2d("Textures/Custle.png"))
		{
			Layer = 6
		};
		_baseSprite.LocalTransform.Scale = new Vector2f(BaseSize, BaseSize);
		_baseScale = _baseSprite.LocalTransform.Scale;

		AddChild(_baseSprite);
		_hitAnimation = new SpriteHitAnimation(
			_baseSprite,
			_baseScale,
			_baseSprite.Color,
			Tweens,
			HitScaleFactor,
			HitDuration);

		float healthBarWidth = BaseSize;
		float healthBarHeight = 6f;
		float healthBarOffsetY = BaseSize * 0.65f;

		_healthBar = new HealthBar(
			renderer, BaseMaxHealth, healthBarWidth, healthBarHeight, new Vector2f(0f, healthBarOffsetY), 7);

		AddChild(_healthBar);
	}

	public float CurrentHealth => _healthBar.Hp;
	public float MaxHealth => _healthBar.MaxHp;
	public bool IsDestroyed => _dead;
	public bool NeedsHealing => !_dead && _healthBar.Hp < _healthBar.MaxHp;
	public bool CanFortify => !_dead && _fortificationLevel < MaxFortificationLevel;

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

		_hitAnimation.Play();

		AudioManager.Ins.PlaySound("base_hit");

		(GameRoot.I.CurrentCamera as Camera2d)?.Shake(0.4f, 5f);

		if (_healthBar.Hp <= 0f && !_dead)
		{
			_dead = true;
			Dead?.Invoke();
		}
	}

	public bool Heal(float amount)
	{
		if (!_dead && NeedsHealing && amount > 0f)
		{
			var result = _healthBar.Heal(amount);

			if(result)
			{
				AudioManager.Ins.PlaySound("base_heal");
			}

			return result;
		}

		return false;
	}

	public bool Fortify(float bonusHealth, bool refillToFull = true)
	{
		if (!CanFortify || bonusHealth <= 0f)
			return false;

		bool changed = _healthBar.IncreaseMaxHealth(bonusHealth, refillToFull);
		if (changed)
		{
			_fortificationLevel++;
			AudioManager.Ins.PlaySound("base_upgrade");
		}

		return changed;
	}
}
