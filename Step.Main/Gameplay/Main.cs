using OpenTK.Mathematics;
using Step.Engine.Graphics;
using Step.Engine;
using Step.Engine.Audio;

namespace Step.Main.Gameplay;

public class Main(Renderer renderer) : GameObject("Root")
{
	private Camera2d? _camera;

	public Action? OnFinish;

	protected override void OnStart()
	{
		renderer.SetBackground(new Color4<Rgba>(0.737f, 0.718f, 0.647f, 1.0f));

		_camera = GetChildOf<Camera2d>();

		AudioManager.Ins.PlaySound("start");
		AudioManager.Ins.PlaySound("main_theme", true);
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
