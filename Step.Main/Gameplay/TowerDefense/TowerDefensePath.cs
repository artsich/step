using System;
using Step.Engine;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class TowerDefensePath : GameObject
{
	private readonly Renderer _renderer;
	private readonly List<Sprite2d> _pathTiles = [];

	private readonly IReadOnlyList<Vector2f> _pathPoints;
	private readonly float _pathWidth;
	private readonly Vector4f _pathColor = new(0.545f, 0.271f, 0.075f, 1f);

	public TowerDefensePath(Renderer renderer, Level level) : base(nameof(TowerDefensePath))
	{
		_renderer = renderer;
		_pathWidth = level.PathWidth;
		_pathPoints = level.PathPoints;
		RebuildPath();
	}

	private void RebuildPath()
	{
		foreach (var tile in _pathTiles)
		{
			RemoveChild(tile);
		}
		_pathTiles.Clear();

		if (_pathPoints.Count == 0)
			return;

		foreach (var position in _pathPoints)
		{
			var tileSprite = new Sprite2d(_renderer, _renderer.DefaultWhiteTexture)
			{
				Color = _pathColor,
				LocalTransform = new Transform
				{
					Position = position,
					Scale = new Vector2f(_pathWidth, _pathWidth)
				}
			};
			
			_pathTiles.Add(tileSprite);
			AddChild(tileSprite);
		}
	}
}

