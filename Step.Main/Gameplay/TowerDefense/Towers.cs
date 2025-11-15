using Step.Engine;
using Step.Engine.Graphics;
using Step.Main.Gameplay.TowerDefense.Core;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class Towers : GameObject
{
	private readonly Renderer _renderer;
	private readonly List<Sprite2d> _gridCells = [];

	private readonly float _cellSize;

	private Vector4f CellColor { get; } = new(0f, 1f, 0f, 0.4f);

	private IReadOnlyList<Vector2f> TowerPlaces { get; }

	public Towers(Renderer renderer, Level level) : base(nameof(Towers))
	{
		_renderer = renderer;
		_cellSize = level.TowerCellSize;
		TowerPlaces = level.TowerPlaces;
		RebuildGrid();
	}

	private void RebuildGrid()
	{
		foreach (var cell in _gridCells)
		{
			CallDeferred(() => RemoveChild(cell));
		}
		_gridCells.Clear();

		foreach (var position in TowerPlaces)
		{
			var cell = new Sprite2d(_renderer, Assets.LoadTexture2d("Textures\\spr_tower_lightning_tower.png"))
			{
				LocalTransform = new Transform
				{
					Position = position,
					Scale = new Vector2f(_cellSize*0.8f, _cellSize)
				}
			};
			
			_gridCells.Add(cell);
			AddChild(cell);
		}
	}
}

