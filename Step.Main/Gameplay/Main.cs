using ImGuiNET;
using OpenTK.Mathematics;
using Step.Main.Gameplay.Spawn;
using Step.Engine.Graphics;
using Step.Engine;
using Step.Engine.Audio;

namespace Step.Main.Gameplay;

public class Main(Spawner spawner, Renderer renderer)
	: GameObject("Root"), IGameScene
{
	private Player? _player;
	private Camera2d? _camera;
	private int _score;

	public Action? OnFinish;

	public Player Player => _player!;

	protected override void OnStart()
	{
		renderer.SetBackground(new Color4<Rgba>(0.737f, 0.718f, 0.647f, 1.0f));

		_player = GetChildOf<Player>();
		_camera = GetChildOf<Camera2d>();

		_player.OnPlayerHeal += () => AudioManager.Ins.PlaySound("player_heal");
		_player.OnThingTaken += (_) => AudioManager.Ins.PlaySound("thing_taken");
		_player.OnThingTaken += (_) => _score++;

		_player.OnDamage += () =>
		{
			_camera.Shake(magnitude: 2f, duration: 0.5f);
			AudioManager.Ins.PlaySound("player_take_damage");
		};

		_player.OnDead += () =>
		{
			Console.WriteLine("Game over...");
			OnFinish?.Invoke();
		};

		AudioManager.Ins.PlaySound("start");
		AudioManager.Ins.PlaySound("main_theme", true);
	}

	public int EffectsCount<T>() where T : IEffect
	{
		var fallingEffects = GetChildsOf<Thing>().Where(x => x.HasEffect<T>()).Count();
		return Player.EffectsCount<T>() + fallingEffects;
	}

	public void KillEnemies()
	{
		Defer(() =>
		{
			var things = GetChildsOf<Thing>().Where(t => !t.IsFriend).ToArray();
			_score += things.Length;
			foreach (var thing in things)
			{
				RemoveChild(thing);
			}
		});

		AudioManager.Ins.PlaySound("kill_all");
		_camera.Shake(magnitude: 5f, duration: 1f);
	}

	protected override void OnUpdate(float deltaTime)
	{
		var spawnedThing = spawner.Get(deltaTime, this);
		if (spawnedThing is not null)
		{
			// start must be called in another place?
			spawnedThing.Start();
			Defer(() => AddChild(spawnedThing));
		}

		var fallingThings = GetChildsOf<Thing>();

		var playerBox = Player.Box;
		foreach (var thing in fallingThings)
		{
			if (thing.BoundingBox.Contains(playerBox))
			{
				if (thing.IsFriend)
				{
					Player.Damage(1);
				}
				else
				{
					Player.Take(thing);
				}

				Defer(() => RemoveChild(thing));
			}
			else if (thing.BoundingBox.Max.Y < -90f)
			{
				if (!thing.IsFriend)
				{
					Player.Damage(1);
				}

				Defer(() => RemoveChild(thing));
			}
		}
	}

	protected override void OnDebugDraw()
	{
		ImGui.SeparatorText("Game info");
		ImGui.Text($"Score: {_score}");

		var fallingThings = GetChildsOf<Thing>();
		ImGui.Text($"Falling things: {fallingThings.Count()}");

		if (ImGui.Button("Hit player"))
		{
			Player.Damage(1);
		}

		if (ImGui.TreeNodeEx("Spawner", ImGuiTreeNodeFlags.DefaultOpen))
		{
			var thingsSpeed = spawner.Speed;
			ImGui.SliderFloat("Things speed", ref thingsSpeed, 1f, 200f);
			spawner.Speed = thingsSpeed;

			var spawnTimeInterval = spawner.TimeInterval;
			ImGui.SliderFloat("Spawn time", ref spawnTimeInterval, 0.01f, 1f);
			spawner.TimeInterval = spawnTimeInterval;

			if (ImGui.Button(spawner.Enabled ? "Disable spawn" : "Enable spawn"))
			{
				spawner.Enabled = !spawner.Enabled;
			}
			ImGui.TreePop();
		}
	}
}
