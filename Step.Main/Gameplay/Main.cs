using Step.Engine.Graphics;
using Step.Engine;
using Step.Engine.Audio;
using Step.Main.Gameplay.Actors;

namespace Step.Main.Gameplay;

public class Main(Renderer renderer) : GameObject("Root")
{
	private Camera2d? _camera;
	private Player _player;

	public Action? OnFinish;

	protected override void OnStart()
	{
		renderer.SetBackground(Colors.Background);

		_camera = GetChildOf<Camera2d>();
		_player = GetChildOf<Player>();
		_player.OnDeath += OnPlayerDeath;
		_player.OnDamage += OnPlayerDamage;

		AudioManager.Ins.PlaySound("start");
		AudioManager.Ins.PlaySound("main_theme", true);
	}

	private void OnPlayerDamage()
	{
		_camera!.Shake(2f, 0.2f);
	}

	protected override void OnEnd()
	{
		_player.OnDeath -= OnPlayerDeath;
		_player.OnDamage -= OnPlayerDamage;
	}

	private void OnPlayerDeath()
	{
		OnFinish?.Invoke();
	}

	protected override void OnUpdate(float deltaTime)
	{
	}

	protected override void OnRender()
	{
		renderer.SetCamera(_camera!);
	}

	protected override void OnDebugDraw()
	{
	}
}
