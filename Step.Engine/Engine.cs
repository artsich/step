using ImGuiNET;
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
using System.Diagnostics;

namespace Step.Engine;

public abstract class RenderResult(string name) : GameObject(name)
{
	public abstract Texture2d ResultTexture { get; }
}

/*
  Input must be decoupled from engine cycle.
 */
public class Engine(WindowOptions windowOptions)
{
	private ImGuiController _imGuiController;
	private EditorUI _editorUI;

	private IWindow _window;
	private static GL GL => Ctx.GL;

	private IInputContext _inputContext;

	public IKeyboard Keyboard => _inputContext.Keyboards.First(x => x.IsConnected);

	public IMouse Mouse => _inputContext.Mice.First(x => x.IsConnected);

	private Input _gameInput;

	private bool _editorEnabled = false;
	private float _lastUpdateTime;

	private List<IEditorView> _editors = [];
	private List<Func<Engine, IEditorView>> _editorFactories = [];

	private bool _gameLoopPaused;
	private float _audioMasterVolume = 0.15f;
	private Func<Engine, RenderResult> _gameCreator;

	public IWindow Window => _window;

	public Input Input => _gameInput;

	public Renderer Renderer { get; private set; }

	public IReadOnlyList<IEditorView> Editors => _editors;

	public Engine AddEditor(Func<Engine, IEditorView> factory)
	{
		_editorFactories.Add(factory);
		return this;
	}

	public void SetGameLoopPaused(bool paused)
	{
		_gameLoopPaused = paused;
	}

	public void SetMasterVolume(float volume)
	{
		_audioMasterVolume = volume;
		AudioManager.Ins.SetMasterVolume(_audioMasterVolume);
	}

	public void ReloadGame()
	{
		if (_gameCreator != null)
		{
			GameRoot.I.SetScene(_gameCreator(this));
		}
	}

	public float LastUpdateTime { get; private set; }

	public void Run(Func<Engine, RenderResult> gameCreator)
	{
		using (_window = Silk.NET.Windowing.Window.Create(windowOptions))
		{
			_window.Load += () =>
			{
				InitSystems();
				_gameCreator = gameCreator;
				var game = gameCreator(this);
				GameRoot.I.SetScene(game);
				_editorFactories.ForEach(fact => _editors.Add(fact(this)));
				_editorUI = new EditorUI(_imGuiController, _editors);
			};

			_window.Closing += () =>
			{
				GameRoot.I.Scene.End();
				UnloadSystems();
			};

			_window.Render += (dt) =>
			{
				Ctx.GL.Clear(ClearBufferMask.ColorBufferBit
					| ClearBufferMask.DepthBufferBit
					| ClearBufferMask.StencilBufferBit);
				GameRoot.I.Draw();

				if (_editorEnabled)
				{
					// todo: should i update editor here?
					_editorUI.Update((float)dt);
					_editorUI.Render();
				}
				else
				{
					var renderResult = (GameRoot.I.Scene as RenderResult);
					Debug.Assert(renderResult != null);

					Renderer.DrawScreenRectNow(renderResult.ResultTexture);
				}
			};

			_window.Update += (dt) =>
			{
				_lastUpdateTime = (float)dt;
				LastUpdateTime = (float)dt;
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

#if DEBUG
				if (Input.IsKeyJustPressed(Key.GraveAccent))
				{
					_editorEnabled = !_editorEnabled;
				}
#endif
				CheckWindowStateToggle();

				if (_editorEnabled)
				{
					foreach (var editor in _editors)
					{
						editor.Update((float)dt);
					}
				}
				else
				{
					if (OperatingSystem.IsMacOS())
					{
						_gameInput.SetWindowSize((Vector2f)_window.FramebufferSize/2);
					}
					else
					{
						_gameInput.SetWindowSize((Vector2f)_window.FramebufferSize);
					}
					_gameInput.SetMouseOffset(Vector2f.Zero);
				}

				if (!_gameLoopPaused)
				{
					_gameInput.Update((float)dt);
					GameRoot.I.Update((float)dt);
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
		var imguiFontScale = 1f;
		if (!OperatingSystem.IsMacOS())
		{
			imguiFontScale = WindowExt.GetScale();
		}
		
		_imGuiController = new ImGuiController(
			GL,
			_window,
			_inputContext = _window.CreateInput(),
			() =>
			{
				var io = ImGui.GetIO();
				LoadDefaultImGuiFont(imguiFontScale, io);
				io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
			}
		);
		Ctx.PrintOpenGLInfo();

		var screenSize = new Vector2i(_window.FramebufferSize.X, _window.FramebufferSize.Y);
		Renderer = new Renderer(screenSize.X, screenSize.Y, Ctx.GL);
		Renderer.Load();

		_gameInput = new Input(_inputContext);

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
				(GameRoot.I.CurrentCamera as Camera2d)?.Zoom(scale);
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

	private static unsafe void LoadDefaultImGuiFont(float fontScale, ImGuiIOPtr io)
	{
		byte[] fontData = EmbeddedResourceLoader.LoadAsBytes("Step.Engine.EngineData.Fonts.ProggyClean.ttf");

		fixed (byte* fontDataPtr = fontData)
		{
			io.Fonts.AddFontFromMemoryTTF(
				new IntPtr(fontDataPtr),
				fontData.Length,
				(int)(13 * fontScale));
		}
	}
}
