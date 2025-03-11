using ImGuiNET;
using Serilog;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using StbImageSharp;
using Step.Engine.Audio;
using Step.Engine.Collisions;
using Step.Engine.Editor;
using Step.Engine.Graphics;
using System.Runtime.InteropServices;

namespace Step.Engine;

public interface IGame
{
	void Load(Engine engine);

	void Unload();

	void Render(float dt);

	void Update(float dt);

	void ImGuiRender(float dt);
}

public class Engine(WindowOptions windowOptions)
{
	private ImGuiController _imGuiController;

	private IWindow _window;
	private GL GL;

	private IInputContext _inputContext;

	private IKeyboard Keyboard => _inputContext.Keyboards.First(x => x.IsConnected);

	public IMouse Mouse => _inputContext.Mice.First(x => x.IsConnected);

	private Input _gameInput;

	private bool _gameLoopPaused;
	private bool _showImGui = true;
	private float _lastUpdateTime;

	private List<IEditorView> _editors = [];
	private float _audioMasterVolume = 0.15f;

	public IWindow Window => _window;

	public Input Input => _gameInput;

	public Renderer Renderer { get; private set; }

	public void AddEditor(IEditorView editor) => _editors.Add(editor);

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
				game.Render((float)dt);

				if (_showImGui)
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

					game.ImGuiRender((float)dt);
					_imGuiController.Render();
				}
			};

			_window.Update += (dt) =>
			{
				_lastUpdateTime = (float)dt;
				if (Input.IsKeyJustPressed(Key.Escape))
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
					_showImGui = !_showImGui;
				}

				CheckWindowStateToggle();

				AudioManager.Ins.SetMasterVolume(_audioMasterVolume);

				if (_showImGui)
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
					game.Update((float)dt);
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

		Ctx.GL = GL = _window.CreateOpenGL();

		if (OperatingSystem.IsWindows())
		{
			GL.Enable(EnableCap.DebugOutput);
			GL.Enable(EnableCap.DebugOutputSynchronous);
			GL.DebugMessageCallback(GlDebugCallback.FuncPtr, in IntPtr.Zero);
		}

		_imGuiController = new ImGuiController(
			GL,
			_window,
			_inputContext = _window.CreateInput(),
			new ImGuiFontConfig("Assets\\ProggyClean.ttf", fontSize: 13 * 2),
			() =>
			{
				var io = ImGui.GetIO();
				io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
			}
		);

		var screenSize = new Vector2i(_window.FramebufferSize.X, _window.FramebufferSize.Y);
		Renderer = new Renderer(screenSize.X, screenSize.Y, Ctx.GL);
		Renderer.Load();

		AudioManager.Ins.SetMasterVolume(_audioMasterVolume);

		_gameInput = new Input(Mouse, Keyboard);
	}

	private void UnloadSystems()
	{
		Renderer.Unload();
	}

	private void CheckWindowStateToggle()
	{
		if (Input.IsKeyPressed(Key.AltLeft))
		{
			if (Input.IsKeyJustReleased(Key.Enter))
			{
				if (_window.WindowState == WindowState.Fullscreen)
				{
					_window.WindowState = WindowState.Normal;
				}
				else
				{
					_window.WindowState = WindowState.Fullscreen;
				}
			}
		}
	}

	private static class GlDebugCallback
	{
		public readonly static DebugProc FuncPtr = GLDebugCallback;
		private const uint GL_DEBUG_CALLBACK_APP_MAKER_ID = 0;

		private static void GLDebugCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userParam)
		{
			if (source == GLEnum.DebugSourceApplication && id == GL_DEBUG_CALLBACK_APP_MAKER_ID)
			{
				return;
			}

			string text = "OpenGL: " + Marshal.PtrToStringAnsi(message, length);
			switch (severity)
			{
				case GLEnum.DebugSeverityLow:
					Log.Logger.Information(text);
					break;
				case GLEnum.DebugSeverityMedium:
					Log.Logger.Warning(text);
					break;
				case GLEnum.DebugSeverityHigh:
					Log.Logger.Error(text);
					break;
				case GLEnum.DebugSeverityNotification:
					if (id == 131185) return; // Buffer detailed info, NVIDIA
					Log.Logger.Information(text);
					break;
				case GLEnum.DontCare:
				default:
					break;
			}
		}
	}
}
