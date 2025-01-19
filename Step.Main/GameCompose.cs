using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Serilog;
using StbImageSharp;
using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Collisions;
using Step.Engine.Editor;
using Step.Engine.Graphics;
using Step.Main.Gameplay;

namespace Step.Main;

public class GameCompose : GameWindow
{
	private const float TargetAspectRatio = 16f / 9f;
	private const float InverseTargetAspectRatio = 1f / TargetAspectRatio;

	private const float GameCameraWidth = 320f;
	private const float GameCameraHeight = GameCameraWidth * InverseTargetAspectRatio;

	private bool _paused = false;
	private bool _showImGui = true;

	private float _lastUpdateTime;
	private float _audioMasterVolume = 0.02f;

	private Renderer _renderer;
	private ImGuiController _controller;
	private RenderTarget2d _gameRenderTarget;

	private readonly List<IEditorView> _editors = [];

	private Camera2d _mainCamera;

	public GameCompose(
		GameWindowSettings gameWindowSettings,
		NativeWindowSettings nativeWindowSettings)
		: base(gameWindowSettings, nativeWindowSettings)
	{
	}

	protected override void OnLoad()
	{
		base.OnLoad();
		MouseWheel += GameMouseWheel;

		StbImage.stbi_set_flip_vertically_on_load(1);

		_controller = new ImGuiController(
			ClientSize.X, ClientSize.Y,
			"Assets\\ProggyClean.ttf", 13.0f, this.GetDpi());

		_renderer = new Renderer(ClientSize.X, ClientSize.Y);
		_renderer.Load();

		_gameRenderTarget = new RenderTarget2d(ClientSize.X, ClientSize.Y, true);

		_renderer.SetBackground(new Color4<Rgba>(0.737f, 0.718f, 0.647f, 1.0f));
		LoadAssets();
		ReloadGame();

		_editors.Add(new ParticlesEditor(ClientSize, _mainCamera));
	}

	private void LoadAssets()
	{
		AudioManager.Ins.LoadSound("start", "Music\\ok_lets_go.mp3");
		AudioManager.Ins.LoadSound("main_theme", "Music\\main_theme.mp3");
		AudioManager.Ins.LoadSound("player_hurt", "Music\\sword\\player_hurt.wav");
		AudioManager.Ins.LoadSound("wall_collision", "Music\\wall_collision.mp3");

		AudioManager.Ins.SetMasterVolume(_audioMasterVolume);
	}

	private void ReloadGame()
	{
		var width = GameCameraWidth;
		var height = GameCameraHeight;
		var camera = new Camera2d(width, height);

		var root = new Gameplay.Main(_renderer);
		root.AddChild(camera);

		root.OnFinish += () =>
		{
			Console.Clear();
			Log.Logger.Information("Reloading...");
			ReloadGame();
		};

		GameRoot.I.SetScene(root);
		_mainCamera = root.GetChildOf<Camera2d>();
	}

	protected override void OnRenderFrame(FrameEventArgs e)
	{
		base.OnRenderFrame(e);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		_renderer.PushRenderTarget(_gameRenderTarget);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		GameRoot.I.Draw();

		_renderer.Flush();
		_renderer.PopRenderTarget();

		if (_showImGui)
		{
			ImGuiRender(e);
		}
		else
		{
			_renderer.DrawScreenRectNow(_gameRenderTarget.Color);
		}

		SwapBuffers();
	}

	private void ImGuiRender(FrameEventArgs e)
	{
		_controller.Update(this, (float)e.Time);

		ImGui.DockSpaceOverViewport();

		if (ImGui.Begin("Some"))
		{
			if (ImGui.Button("Clear console"))
			{
				Console.Clear();
			}

			if (ImGui.Button("Game & Assets reload"))
			{
				UnloadAssets();
				LoadAssets();

				//UnloadGame();
				ReloadGame();
			}

			if (ImGui.Button(_paused ? "Paused" : "Un pause"))
			{
				_paused = !_paused;
			}

			ImGui.End();
		}

		if (ImGui.Begin("Audio Settings"))
		{
			ImGui.SliderFloat("Master volume", ref _audioMasterVolume, 0f, 1f);
			ImGui.End();
		}

		if (ImGui.Begin("Assets"))
		{
			if (ImGui.BeginTabBar("Main Tabs"))
			{
				foreach (var editor in _editors)
				{
					if (ImGui.BeginTabItem(editor.Name))
					{
						editor.Draw();
						ImGui.EndTabItem();
					}
				}
				ImGui.EndTabBar();
			}
			ImGui.End();
		}

		if (ImGui.Begin("Performance"))
		{
			var ms = e.Time * 1000;
			var fps = 1000 / ms;
			ImGui.Text($"Render time: {ms:F2}ms | {fps:F2}fps");
			ImGui.Text($"Update time: {_lastUpdateTime * 1000:F2}ms");

			ImGui.Separator();
			ImGui.Text($"Collision shapes: {CollisionSystem.Ins.Count}");
			ImGui.End();
		}

		ImGui.ShowDebugLogWindow();

		if (ImGui.Begin("Game render", ImGuiWindowFlags.NoScrollbar))
		{
			var imgSize = StepMath
				.AdjustToAspect(
					TargetAspectRatio,
					ImGui.GetContentRegionAvail().FromSystem())
				.ToSystem();

			ImGui.Image(_gameRenderTarget.Color.Handle, imgSize, new(0f, 1f), new(1f, 0f));

			ImGui.End();
		}

		if (ImGui.Begin("Scene"))
		{
			GameRoot.I.DebugDraw();
			ImGui.End();
		}

		_controller.Render();
		ImGuiController.CheckGLError("End of frame");
	}

	protected override void OnUpdateFrame(FrameEventArgs e)
	{
		base.OnUpdateFrame(e);
		float dt = (float)e.Time;
		_lastUpdateTime = dt;

		if (KeyboardState.IsKeyDown(Keys.Escape))
		{
			Close();
		}

		if (KeyboardState.IsKeyPressed(Keys.P))
		{
			_paused = !_paused;
		}

		if (KeyboardState.IsKeyPressed(Keys.GraveAccent))
		{
			_showImGui = !_showImGui;
		}

		CheckWindowStateToggle();

		AudioManager.Ins.SetMasterVolume(_audioMasterVolume);

		if (_showImGui)
		{
			foreach (var editor in _editors)
			{
				editor.Update(dt);
			}
		}

		if (!_paused)
		{
			GameRoot.I.Update(dt);
		}
	}

	private void CheckWindowStateToggle()
	{
		if (KeyboardState.IsKeyDown(Keys.LeftAlt))
		{
			if (KeyboardState.IsKeyPressed(Keys.Enter))
			{
				if (WindowState == WindowState.Fullscreen)
				{
					WindowState = WindowState.Normal;
				}
				else
				{
					WindowState = WindowState.Fullscreen;
				}
			}
		}
	}

	protected override void OnTextInput(TextInputEventArgs e)
	{
		base.OnTextInput(e);

		if (_showImGui)
		{
			_controller.PressChar((char)e.Unicode);
		}
	}

	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		base.OnMouseWheel(e);
		if (_showImGui)
		{
			_controller.MouseScroll(e.Offset);
		}
	}

	protected override void OnResize(ResizeEventArgs e)
	{
		base.OnResize(e);
		GL.Viewport(0, 0, e.Width, e.Height);
		_controller.WindowResized(e.Width, e.Height);
	}

	private void UnloadAssets()
	{
		AudioManager.Ins.UnloadSounds();
	}

	protected override void OnUnload()
	{
		UnloadAssets();

		AudioManager.Ins.Dispose();
		_renderer.Unload();
		base.OnUnload();
	}

	private void GameMouseWheel(MouseWheelEventArgs obj)
	{
		var scale = 0.1f;
		if (obj.OffsetY != 0f)
		{
			scale *= Math.Sign(obj.OffsetY);
			_mainCamera.Zoom(scale);
		}
	}
}
