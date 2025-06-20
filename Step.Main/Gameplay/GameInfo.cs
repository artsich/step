using System.Text.Json;
using System.Text.Json.Serialization;

namespace Step.Main.Gameplay;

public enum Coin
{
	Circle,
	Cross,
	Glider,
}

public class GameInfo
{
	[JsonInclude]
	private Dictionary<Coin, int> Coins { get; set; } = [];

	public int MaxHp { get; set; }

	public int MaxSpeed { get; set; }

	public void AddCoin(Coin coin, int count)
	{
		Coins.TryAdd(coin, 0);
		Coins[coin] += count;
	}

	public int GetCoin(Coin coin)
	{
		return Coins.TryGetValue(coin, out var coinValue) ? coinValue : 0;
	}

	public void SaveToFile(string filePathJson)
	{
		var json = JsonSerializer.Serialize(this);
		File.WriteAllText(filePathJson, json);
	}

	public static GameInfo FromFile(string filePathJson)
	{
		if (!Path.Exists(filePathJson))
		{
			var result = new GameInfo();
			return result;
		}

		var json = File.ReadAllText(filePathJson);
		return JsonSerializer.Deserialize<GameInfo>(json) ?? throw new InvalidDataException("Deserialization is failed...");
	}
}
