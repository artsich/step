using Step.Engine;
using Step.Engine.Graphics;
using Step.Main.Gameplay.TowerDefense.Core;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class Towers : GameObject
{
	private readonly Renderer _renderer;
	private readonly Input _input;
	private readonly Spawns _spawns;
	private readonly TowerEconomy _economy;
	private readonly List<Tower> _towers = [];
	private readonly List<TowerCell> _cells = [];

	private readonly float _cellSize;
	private bool _placementEnabled = true;

	private IReadOnlyList<Vector2f> TowerPlaces { get; }

	public Towers(Renderer renderer, Input input, Level level, Spawns spawns, TowerEconomy economy) 
		: base(nameof(Towers))
	{
		_renderer = renderer;
		_input = input;
		_spawns = spawns ?? throw new ArgumentNullException(nameof(spawns));
		_economy = economy ?? throw new ArgumentNullException(nameof(economy));
		_cellSize = level.TowerCellSize;
		TowerPlaces = level.TowerPlaces;
		RebuildGrid();
	}

	private void RebuildGrid()
	{
		foreach (var cell in _cells)
		{
			cell.Clicked -= HandleCellClicked;
			var cellToRemove = cell;
			CallDeferred(() => RemoveChild(cellToRemove));
		}
		_cells.Clear();

		foreach (var tower in _towers)
		{
			CallDeferred(() => RemoveChild(tower));
		}
		_towers.Clear();

		foreach (var position in TowerPlaces)
		{
			var cell = new TowerCell(_renderer, _input, position, _cellSize);
			cell.Clicked += HandleCellClicked;
			cell.InteractionEnabled = _placementEnabled;

			_cells.Add(cell);

			var cellToAdd = cell;
			CallDeferred(() => AddChild(cellToAdd));
		}
	}

	private void HandleCellClicked(TowerCell cell)
	{
		if (!_placementEnabled)
			return;

		if (cell.IsOccupied)
		{
			SellTower(cell);
			return;
		}

		if (!_economy.TryPurchaseTower())
			return;

		var tower = new Tower(
			_renderer,
			cell.Position,
			_cellSize,
			() => _spawns.ActiveEnemies);

		if (cell.TryOccupy(tower))
		{
			_towers.Add(tower);
			CallDeferred(() => AddChild(tower));
		}
		else
		{
			_economy.RefundTowerPurchase();
		}
	}

	private void SellTower(TowerCell cell)
	{
		var tower = cell.ReleaseTower();
		if (tower == null)
			return;

		_towers.Remove(tower);
		_economy.RefundTowerPurchase();
		tower.QueueFree();
	}

	public void SetPlacementEnabled(bool enabled)
	{
		if (_placementEnabled == enabled)
			return;

		_placementEnabled = enabled;

		foreach (var cell in _cells)
		{
			cell.InteractionEnabled = enabled;
		}
	}
}

