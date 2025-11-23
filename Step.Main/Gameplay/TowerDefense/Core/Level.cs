namespace Step.Main.Gameplay.TowerDefense.Core;
public readonly struct SpawnSettings
{
	public int EnemyCount { get; }
	public float SpawnFrequency { get; }
	public float SpawnIntervalSeconds => 1f / SpawnFrequency;

	internal SpawnSettings(int enemyCount, float spawnFrequency)
	{
		EnemyCount = enemyCount;
		SpawnFrequency = spawnFrequency;
	}
}

public class Level
{
	private const char PathChar = 'P';
	private const char SpawnChar = 'S';
	private const char BaseChar = 'B';
	private const char TowerChar = 'T';

	private readonly List<Vector2f> _pathPoints = [];
	private readonly List<Vector2f> _spawnPositions = [];
	private readonly List<Vector2f> _towerPlaces = [];
	private readonly Dictionary<Vector2f, IReadOnlyList<Vector2f>> _pathsFromSpawn = new();

	private bool _baseDefined;
	private SpawnSettings? _spawnSettings;
	private WaveConfig[]? _waves;

	private float TileSize { get; set; } = 30f;

	public float PathWidth => TileSize;
	public float TowerCellSize => TileSize;

	public IReadOnlyList<Vector2f> PathPoints => _pathPoints;
	
	public IReadOnlyList<Vector2f> SpawnPositions => _spawnPositions;
	
	public IReadOnlyList<Vector2f> TowerPlaces => _towerPlaces;

	public Vector2f BasePosition { get; private set; }

	public SpawnSettings Spawn => _spawnSettings ?? throw new InvalidOperationException("Spawn settings are not configured. Call ConfigureSpawn before Build().");

	public IReadOnlyList<WaveConfig> Waves => _waves ?? throw new InvalidOperationException("Waves are not configured. Call ConfigureWaves before Build().");

	public Level ConfigureSpawn(int enemyCount, float spawnFrequency)
	{
		if (enemyCount <= 0)
			throw new ArgumentOutOfRangeException(nameof(enemyCount), "Enemy count must be greater than zero.");

		if (spawnFrequency <= 0f || float.IsNaN(spawnFrequency) || float.IsInfinity(spawnFrequency))
			throw new ArgumentOutOfRangeException(nameof(spawnFrequency), "Spawn frequency must be a finite value greater than zero.");

		_spawnSettings = new SpawnSettings(enemyCount, spawnFrequency);
		return this;
	}

	public Level ConfigureWaves(params WaveConfig[] waves)
	{
		if (waves == null || waves.Length == 0)
			throw new ArgumentException("Waves array cannot be null or empty.", nameof(waves));

		_waves = waves;
		return this;
	}
	
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

				if (cell == '.')
				{
					continue;
				}

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

		if (_waves == null || _waves.Length == 0)
			throw new InvalidOperationException("Waves must be configured before building the level. Call ConfigureWaves before Build().");

		return this;
	}

	private void Reset()
	{
		_pathPoints.Clear();
		_spawnPositions.Clear();
		_towerPlaces.Clear();
		_pathsFromSpawn.Clear();
		_baseDefined = false;
		BasePosition = Vector2f.Zero;
		_spawnSettings = null;
		_waves = null;
	}

	private static bool IsKnownCell(char cell)
	{
		return cell == PathChar
			   || cell == SpawnChar
			   || cell == BaseChar
			   || cell == TowerChar;
	}

	public IReadOnlyList<Vector2f> GetPathFromSpawn(Vector2f spawnPosition)
	{
		if (!_baseDefined)
			throw new InvalidOperationException("Level must have a base defined before requesting a path.");

		if (!_spawnPositions.Contains(spawnPosition))
			throw new ArgumentException("Spawn position is not part of the current level.", nameof(spawnPosition));

		if (_pathsFromSpawn.TryGetValue(spawnPosition, out var cachedPath))
			return cachedPath;

		var path = PathFinder.BuildPath(spawnPosition, BasePosition, _pathPoints, TileSize);
		_pathsFromSpawn[spawnPosition] = path;
		return path;
	}
}

