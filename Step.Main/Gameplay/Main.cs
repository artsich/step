using ImGuiNET;
using OpenTK.Mathematics;
using Step.Main.Audio;
using Step.Main.Gameplay.Spawn;
using Step.Main.Graphics;

namespace Step.Main.Gameplay;

public class Main(Spawner spawner, Renderer renderer)
	: GameObject("Root"), IGameScene
{
	private Player? _player;
	private Camera2d? _camera;
	private int _score;

	public Player Player => _player!;

	protected override void OnStart()
	{
		renderer.SetBackground(new Color4<Rgba>(0.737f, 0.718f, 0.647f, 1.0f));

		_player = GetChildOf<Player>();
		_camera = GetChildOf<Camera2d>();

		_player.OnThingTaken += (_) => _score++;
	}

	public int EffectsCount<T>() where T : IEffect
	{
		var fallingEffects = GetChildsOf<Thing>().Where(x => x.HasEffect<T>()).Count();
		return Player.EffectsCount<T>() + fallingEffects;
	}

	public void KillThings()
	{
		Defer(() =>
		{
			var things = GetChildsOf<Thing>().ToArray();
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
				_player.Take(thing);

				Defer(() => RemoveChild(thing));
			}
			else if (thing.BoundingBox.Max.Y < -90f)
			{
				Defer(() => RemoveChild(thing));
				Player.Damage(1);
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
