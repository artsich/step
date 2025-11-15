namespace Step.Main.Gameplay.TowerDefense.Core;

public static class PathFinder
{
	public static IReadOnlyList<Vector2f> BuildPath(Vector2f start, Vector2f goal, IEnumerable<Vector2f> pathTiles, float tileSize)
	{
		if (tileSize <= 0f)
			throw new ArgumentOutOfRangeException(nameof(tileSize), "Tile size must be greater than zero.");

		var walkable = new HashSet<Vector2f>(pathTiles);

		if (!walkable.Contains(start))
			throw new InvalidOperationException("Start position is not aligned with any path tile.");

		if (!walkable.Contains(goal))
			throw new InvalidOperationException("Goal position is not aligned with any path tile.");

		var frontier = new Queue<Vector2f>();
		var cameFrom = new Dictionary<Vector2f, Vector2f>();
		var visited = new HashSet<Vector2f> { start };

		frontier.Enqueue(start);

		while (frontier.Count > 0)
		{
			var current = frontier.Dequeue();

			if (current.Equals(goal))
				break;

			foreach (var neighbor in EnumerateNeighbors(current, tileSize))
			{
				if (!walkable.Contains(neighbor) || !visited.Add(neighbor))
					continue;

				cameFrom[neighbor] = current;
				frontier.Enqueue(neighbor);
			}
		}

		if (!start.Equals(goal) && !cameFrom.ContainsKey(goal))
			throw new InvalidOperationException("No path exists from start to goal.");

		var path = new List<Vector2f> { goal };
		var currentNode = goal;

		while (!currentNode.Equals(start))
		{
			if (!cameFrom.TryGetValue(currentNode, out currentNode))
				throw new InvalidOperationException("Failed to reconstruct path from start to goal.");

			path.Add(currentNode);
		}

		path.Reverse();
		return path;
	}

	private static IEnumerable<Vector2f> EnumerateNeighbors(Vector2f position, float step)
	{
		yield return new Vector2f(position.X + step, position.Y);
		yield return new Vector2f(position.X - step, position.Y);
		yield return new Vector2f(position.X, position.Y + step);
		yield return new Vector2f(position.X, position.Y - step);
	}
}

