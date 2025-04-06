using ImGuiNET;
using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Graphics;
using Step.Main.Gameplay.Actors;
using Step.Main.Gameplay.UI;
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
	public Dictionary<Coin, int> Coins { get; private set; } = [];

	public void AddCoin(Coin coin, int count)
	{
		Coins.TryAdd(coin, 0);
		Coins[coin] += count;
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
			return new GameInfo();
		}

		var json = File.ReadAllText(filePathJson);
		return JsonSerializer.Deserialize<GameInfo>(json) ?? throw new InvalidDataException("Deserialization is failed...");
	}
}

public class GameLoop : GameObject
{
	private const string PathToSaveFile = "./Assets/GameSave.json";

	private Camera2d? _camera;
	private Player? _player;
	private Spawner? _spawner;

	private GameInfo _gameInfo;

	public Action? OnFinish;

	public GameLoop(Engine.Engine engine) : base("Root")
	{
		_gameInfo = GameInfo.FromFile(PathToSaveFile);

		AddChild(new GameHud(engine.Renderer, _gameInfo));
	}

	protected override void OnStart()
	{
		_camera = GetChildOf<Camera2d>();
		_player = GetChildOf<Player>();
		_spawner = GetChildOf<Spawner>();

		_spawner.OnSpawn += OnEnemySpawn;

		_player.OnDeath += OnPlayerDeath;
		_player.OnDamage += OnPlayerDamage;

		_player.OnCrossCoinCollected += OnCrossCoinCollected;

		AudioManager.Ins.PlaySound("start");
		AudioManager.Ins.PlaySound("main_theme", true);
	}

	private void OnEnemySpawn(GameObject obj)
	{
		if (obj is GliderEntity)
		{
			_gameInfo.AddCoin(Coin.Glider, 1);
		}
		else if (obj is CircleEnemy)
		{
			_gameInfo.AddCoin(Coin.Circle, 1);
		}
	}

	private void OnCrossCoinCollected()
	{
		_gameInfo.AddCoin(Coin.Cross, 1);
	}

	private void OnPlayerDamage()
	{
		_camera!.Shake(2f, 0.2f);
	}

	protected override void OnEnd()
	{
		_spawner!.OnSpawn -= OnEnemySpawn;

		_player!.OnDeath -= OnPlayerDeath;
		_player.OnCrossCoinCollected -= OnCrossCoinCollected;
		_player.OnDamage -= OnPlayerDamage;

		_gameInfo.SaveToFile(PathToSaveFile);
	}

	private void OnPlayerDeath()
	{
		OnFinish?.Invoke();
	}

	protected override void OnDebugDraw()
	{
		if (ImGui.Begin("Game stat"))
		{
			foreach (var coin in _gameInfo.Coins)
			{
				ImGui.Text($"{coin.Key} : {coin.Value}");
			}

			ImGui.End();
		}
	}
}
