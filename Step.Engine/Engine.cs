﻿using ImGuiNET;
using Serilog;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using StbImageSharp;
using Step.Engine.Audio;
using Step.Engine.Collisions;
using Step.Engine.Editor;
using Step.Engine.Graphics;
using Step.Engine.Logging;

namespace Step.Engine;

public interface IGame
{
	void Load(Engine engine);

	void Unload();

	Texture2d Render(float dt);
}

/*
  Input must be decoupled from engine cycle.
 */
public class Engine(WindowOptions windowOptions)
{
	private const float TargetAspectRatio = 16f / 9f;

	private ImGuiController _imGuiController;

	private IWindow _window;
	private static GL GL => Ctx.GL;

	private IInputContext _inputContext;

	public IKeyboard Keyboard => _inputContext.Keyboards.First(x => x.IsConnected);

	public IMouse Mouse => _inputContext.Mice.First(x => x.IsConnected);

	private Input _gameInput;

	private bool _gameLoopPaused;
	private bool _editorEnabled = false;
	private float _lastUpdateTime;

	private List<IEditorView> _editors = [];
	private float _audioMasterVolume = 0.15f;

	public IWindow Window => _window;

	public Input Input => _gameInput;

	public Renderer Renderer { get; private set; }

	public void AddEditor(IEditorView editor) => _editors.Add(editor);

	public void ClearEditors() => _editors.Clear();

	public void Run(IGame game)
	{
		using (_window = Silk.NET.Windowing.Window.Create(windowOptions))
		{
			_window.Load += () =>
			{
				InitSystems();
				game.Load(this);
			};

			_window.Closing += () =>
			{
				game.Unload();
				UnloadSystems();
			};

			_window.Render += (dt) =>
			{
				Ctx.GL.Clear(ClearBufferMask.ColorBufferBit
					| ClearBufferMask.DepthBufferBit
					| ClearBufferMask.StencilBufferBit);
				var finalImage = game.Render((float)dt);

				if (_editorEnabled)
				{
					_imGuiController.Update((float)dt);
					ImGui.DockSpaceOverViewport();

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

					if (ImGui.Begin("Game controls"))
					{
						if (ImGui.Button("Game & Assets reload"))
						{
							game.Unload();
							game.Load(this);
						}

						ImGui.Text($"Mouse: {_gameInput.MouseWorldPosition.X:F0}, {_gameInput.MouseWorldPosition.Y:F0}");

						ImGui.End();
					}

					if (ImGui.Begin("Audio Settings"))
					{
						ImGui.SliderFloat("Master volume", ref _audioMasterVolume, 0f, 1f);
						ImGui.End();
					}

					if (ImGui.Begin("Engine controls"))
					{
						if (ImGui.Button("Clear console"))
						{
							Console.Clear();
						}

						if (ImGui.Button(_gameLoopPaused ? "Paused" : "Un pause"))
						{
							_gameLoopPaused = !_gameLoopPaused;
						}

						ImGui.End();
					}

					ImGui.ShowDebugLogWindow();

					if (ImGui.Begin("Performance"))
					{
						var ms = dt * 1000f;
						var fps = 1000 / ms;
						ImGui.Text($"Render time: {ms:F2}ms | {fps:F2}fps");
						ImGui.Text($"Update time: {_lastUpdateTime * 1000:F2}ms");

						ImGui.Separator();
						ImGui.Text($"Collision shapes: {CollisionSystem.Ins.Count}");

						ImGui.Separator();
						ImGui.Text($"GPU Draw time: {Renderer.Stats.GpuTimeMs:F5} ms");
						ImGui.Text($"Shaders used: {Renderer.Stats.ActiveShaders}");

						ImGui.End();
					}

					if (ImGui.Begin("Scene"))
					{
						GameRoot.I.DebugDraw();
						ImGui.End();
					}

					if (ImGui.Begin("Game render", ImGuiWindowFlags.NoScrollbar))
					{
						var availRegion = ImGui.GetContentRegionAvail().FromSystem();
						var imgSize = StepMath
							.AdjustToAspect(
								TargetAspectRatio,
								availRegion)
							.ToSystem();

						var headerOffset = new Vector2f(
							(ImGui.GetWindowSize().X - availRegion.X) / 2f,
							ImGui.GetWindowSize().Y - availRegion.Y);

						ImGui.Image((nint)finalImage.Handle, imgSize, new(0f, 1f), new(1f, 0f));

						var windowPos = ImGui.GetWindowPos().FromSystem();
						Input.SetMouseOffset(windowPos + headerOffset);
						Input.SetWindowSize(imgSize.FromSystem());
						ImGui.End();
					}

					_imGuiController.Render();
				}
				else
				{
					Renderer.DrawScreenRectNow(finalImage);
				}
			};

			_window.Update += (dt) =>
			{
				_lastUpdateTime = (float)dt;
				if (Input.IsKeyJustPressed(Key.F1))
				{
					_window.Close();
					return;
				}

				// todo: need to separate this input from engine input...
				//if (Input.IsKeyJustPressed(Key.P))
				//{
				//	_gameLoopPaused = !_gameLoopPaused;
				//}

				if (Input.IsKeyJustPressed(Key.GraveAccent))
				{
					_editorEnabled = !_editorEnabled;
				}

				CheckWindowStateToggle();

				AudioManager.Ins.SetMasterVolume(_audioMasterVolume);

				if (_editorEnabled)
				{
					foreach (var editor in _editors)
					{
						editor.Update((float)dt);
					}
				}
				else
				{
					_gameInput.SetWindowSize((Vector2f)_window.FramebufferSize);
					_gameInput.SetMouseOffset(Vector2f.Zero);
				}

				if (!_gameLoopPaused)
				{
					_gameInput.Update((float)dt);
					GameRoot.I.Update((float)dt);

					//game.Update((float)dt);
				}
			};

			_window.Resize += (e) =>
			{
				GL.Viewport(e);
			};

			_window.Run();
		}
	}

	private void InitSystems()
	{
		StbImage.stbi_set_flip_vertically_on_load(1);

		Log.Logger = new LoggerConfiguration()
			.WriteTo.Async(wt =>
			{
				wt.Console();
				wt.ImGuiDebugLog();
			})
			.CreateLogger();

		Ctx.Init(_window);
		_imGuiController = new ImGuiController(
			GL,
			_window,
			_inputContext = _window.CreateInput(),
			new ImGuiFontConfig(
				"EngineData/Fonts/ProggyClean.ttf", 
				(int)(13 * WindowExt.GetScale())),
			() =>
			{
				var io = ImGui.GetIO();
				io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
			}
		);
		Ctx.PrintOpenGLInfo();

		var screenSize = new Vector2i(_window.FramebufferSize.X, _window.FramebufferSize.Y);
		Renderer = new Renderer(screenSize.X, screenSize.Y, Ctx.GL);
		Renderer.Load();

		AudioManager.Ins.SetMasterVolume(_audioMasterVolume);

		_gameInput = new Input(Mouse, Keyboard);

		Mouse.Scroll += GameMouseWheel;
	}

	private void GameMouseWheel(IMouse _, ScrollWheel scroll)
	{
		if (_editorEnabled)
		{
			var scale = 0.1f;
			if (scroll.Y != 0f)
			{
				scale *= Math.Sign(scroll.Y);
				GameRoot.I.Scene.GetChildOf<Camera2d>().Zoom(scale);
			}
		}
	}

	private void UnloadSystems()
	{
		AudioManager.Ins.Dispose();
		Renderer.Unload();
	}

	private void CheckWindowStateToggle()
	{
		if (Input.IsKeyPressed(Key.AltLeft))
		{
			if (Input.IsKeyJustReleased(Key.Enter))
			{
				if (_window.WindowState == WindowState.Maximized)
				{
					_window.WindowState = WindowState.Normal;
				}
				else
				{
					_window.WindowState = WindowState.Maximized;
				}
			}
		}
	}
}
