using Step.Main.Gameplay.TowerDefense.Core;
using Vector2f = Silk.NET.Maths.Vector2D<float>;

namespace Step.Main.Tests;

public class LevelTests
{
	[Fact]
	public void LoadAndBuild_WithMixedLayout_ConfiguresAllElements()
	{
		const float tileSize = 30f;
		var map = new[]
		{
			"....T....",
			"...PPP...",
			"..TPTP.B.",
			"...P.PTP.",
			".SPPTPPP."
		};

		var level = new Level();
		level.LoadFromStrings(tileSize, map);
		level.ConfigureSpawn(5, 1.5f);
		level.Build();

		var expectedSpawn = EnumerateCells(map, tileSize, 'S').Single();
		var expectedBase = EnumerateCells(map, tileSize, 'B').Single();
		var expectedTowers = EnumerateCells(map, tileSize, 'T').ToList();
		var expectedPath = EnumerateCells(map, tileSize, 'S', 'P', 'B').ToList();

		Assert.Equal(expectedSpawn, Assert.Single(level.SpawnPositions));
		Assert.Equal(expectedBase, level.BasePosition);
		Assert.Equal(expectedTowers, level.TowerPlaces);
		Assert.Equal(expectedPath, level.PathPoints);

		var pathFromSpawn = level.GetPathFromSpawn(expectedSpawn).ToList();

		Assert.Equal(expectedSpawn, pathFromSpawn.First());
		Assert.Equal(expectedBase, pathFromSpawn.Last());

		foreach (var point in pathFromSpawn)
		{
			Assert.Contains(point, expectedPath);
		}

		foreach (var (current, next) in pathFromSpawn.Zip(pathFromSpawn.Skip(1)))
		{
			var delta = next - current;
			Assert.True(delta.X == 0f || delta.Y == 0f, "Each path step must move either horizontally or vertically.");
			Assert.Equal(tileSize, MathF.Abs(delta.X) + MathF.Abs(delta.Y));
		}
	}

	private static IEnumerable<Vector2f> EnumerateCells(string[] map, float tileSize, params char[] targets)
	{
		var targetSet = new HashSet<char>(targets);
		int width = map[0].Length;
		int height = map.Length;

		for (int row = 0; row < height; row++)
		{
			for (int col = 0; col < width; col++)
			{
				if (!targetSet.Contains(map[row][col]))
					continue;

				yield return ToWorld(row, col, width, height, tileSize);
			}
		}
	}

	private static Vector2f ToWorld(int row, int col, int width, int height, float tileSize)
	{
		float originX = -(width - 1) * 0.5f;
		float originY = -(height - 1) * 0.5f;

		return new Vector2f(
			(originX + col) * tileSize,
			(originY + (height - 1 - row)) * tileSize);
	}
}
