using System;
using System.Collections.Generic;
using Serilog.Core;
using Step.Engine;

namespace Step.Main.Gameplay.TowerDefense;

public class Level
{
	private const char PathChar = 'P';
	private const char SpawnChar = 'S';
	private const char BaseChar = 'B';
	private const char TowerChar = 'T';

	private readonly List<Vector2f> _pathPoints = new();
	private readonly List<Vector2f> _spawnPositions = new();
	private readonly List<Vector2f> _towerPlaces = new();

	private bool _baseDefined;

	private float TileSize { get; set; } = 30f;

	public float PathWidth => TileSize;
	public float TowerCellSize => TileSize;

	public IReadOnlyList<Vector2f> PathPoints => _pathPoints;
	public IReadOnlyList<Vector2f> SpawnPositions => _spawnPositions;
	public IReadOnlyList<Vector2f> TowerPlaces => _towerPlaces;
	public Vector2f BasePosition { get; private set; }
	
	public Level LoadFromStrings(float tileSize, params string[] rows)
	{
		if (rows == null || rows.Length == 0)
			throw new ArgumentException("Level map must contain at least one row.", nameof(rows));

		int width = rows[0].Length;
		if (width == 0)
			throw new ArgumentException("Level map rows must not be empty.", nameof(rows));

		foreach (var row in rows)
		{
			if (row.Length != width)
				throw new ArgumentException("All rows in the level map must have the same length.", nameof(rows));
		}

		if (tileSize <= 0f)
			throw new ArgumentOutOfRangeException(nameof(tileSize), "Tile size must be > 0");

		Reset();
		TileSize = tileSize;

		int height = rows.Length;
		float originX = -(width - 1) * 0.5f;
		float originY = -(height - 1) * 0.5f;

		for (int row = 0; row < height; row++)
		{
			for (int col = 0; col < width; col++)
			{
				char cell = rows[row][col];
				if (!IsKnownCell(cell))
				{
					Serilog.Log.Warning("Unknown cell: {Cell}", cell);
					continue;
				}

				float worldX = (originX + col) * TileSize;
				float worldY = (originY + (height - 1 - row)) * TileSize;
				var position = new Vector2f(worldX, worldY);

				switch (cell)
				{
					case PathChar:
						_pathPoints.Add(position);
						break;
					case SpawnChar:
						_pathPoints.Add(position);
						_spawnPositions.Add(position);
						break;
					case BaseChar:
						_pathPoints.Add(position);
						BasePosition = position;
						_baseDefined = true;
						break;
					case TowerChar:
						_towerPlaces.Add(position);
						break;
				}
			}
		}

		return this;
	}

	public Level Build()
	{
		if (_pathPoints.Count == 0)
			throw new InvalidOperationException("Level must have at least one path tile (P/S/B).");

		if (_spawnPositions.Count == 0)
			throw new InvalidOperationException("Level must have at least one spawn tile (S).");

		if (!_baseDefined)
			throw new InvalidOperationException("Level map must contain a base tile (B).");

		return this;
	}

	private void Reset()
	{
		_pathPoints.Clear();
		_spawnPositions.Clear();
		_towerPlaces.Clear();
		_baseDefined = false;
		BasePosition = Vector2f.Zero;
	}

	private static bool IsKnownCell(char cell)
	{
		return cell == PathChar
			   || cell == SpawnChar
			   || cell == BaseChar
			   || cell == TowerChar;
	}
}

