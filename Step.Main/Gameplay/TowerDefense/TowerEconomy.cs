using Step.Engine;

namespace Step.Main.Gameplay.TowerDefense;

public readonly record struct TowerEconomySettings(
	int StartingGold,
	int GoldPerKill,
	int TowerCost)
{
	public static TowerEconomySettings Default => new(50, 25, 50);
}

public sealed class TowerEconomy : GameObject
{
	private readonly Spawns _spawns;
	private readonly TowerEconomySettings _settings;

	private int _currentGold;

	public TowerEconomy(Spawns spawns, TowerEconomySettings settings)
		: base(nameof(TowerEconomy))
	{
		_spawns = spawns ?? throw new ArgumentNullException(nameof(spawns));
		_settings = settings;
		_currentGold = Math.Max(0, settings.StartingGold);
	}

	public event Action<int>? GoldChanged;

	public int CurrentGold => _currentGold;

	public int TowerCost => _settings.TowerCost;

	public bool CanAffordTower => _currentGold >= _settings.TowerCost;

	public bool TryPurchaseTower()
	{
		if (!CanAffordTower)
			return false;

		SpendGold(_settings.TowerCost);
		return true;
	}

	public void RefundTowerPurchase()
	{
		AddGold(_settings.TowerCost);
	}

	protected override void OnStart()
	{
		base.OnStart();
		_spawns.EnemyDied += HandleEnemyDied;
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		_spawns.EnemyDied -= HandleEnemyDied;
	}

	private void HandleEnemyDied(Enemy enemy)
	{
		AddGold(_settings.GoldPerKill);
	}

	private void SpendGold(int amount)
	{
		if (amount <= 0)
			return;

		_currentGold = Math.Max(0, _currentGold - amount);
		GoldChanged?.Invoke(_currentGold);
	}

	private void AddGold(int amount)
	{
		if (amount <= 0)
			return;

		_currentGold += amount;
		GoldChanged?.Invoke(_currentGold);
	}
}

