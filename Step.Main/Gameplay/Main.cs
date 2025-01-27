using OpenTK.Mathematics;
using Step.Engine.Graphics;
using Step.Engine;
using Step.Engine.Audio;

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

		AudioManager.Ins.PlaySound("start");
		AudioManager.Ins.PlaySound("main_theme", true);
	}

	protected override void OnEnd()
	{
		_player.OnDeath -= OnPlayerDeath;
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
