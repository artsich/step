using Step.Engine;
using Step.Engine.Graphics;
using Step.Engine.Graphics.UI;

namespace Step.Main.Gameplay.UI;

public class MainMenu : GameObject
{
	private readonly VContainer _container;
	private readonly Button _continueButton;
	private readonly Camera2d _camera;

	public event Action? OnPlayPressed;
	public event Action? OnContinuePressed;
	public event Action? OnExitPressed;

	private Engine.Engine _engine;
	private readonly Button _playButton;

	public MainMenu(Engine.Engine engine) : base("MainMenu")
	{
		_engine = engine;
		_playButton = new Button("Play", () => OnPlayPressed?.Invoke(), engine.Input, engine.Renderer)
		{
			TextColor = Color.Red,
			FontPath = "Assets/Fonts/Pixellari.ttf",
			TextAlignment = TextAlignment.Center
		};

		_continueButton = new Button("Continue", () => OnContinuePressed?.Invoke(), engine.Input, engine.Renderer)
		{
			TextColor = Color.Red,
			FontPath = "Assets/Fonts/Pixellari.ttf",
			Enabled = false,
			TextAlignment = TextAlignment.Center
		};

		var exitButton = new Button("Exit", () => OnExitPressed?.Invoke(), engine.Input, engine.Renderer)
		{
			FontPath = "Assets/Fonts/Pixellari.ttf",
			TextColor = Color.Red,
			TextAlignment = TextAlignment.Center
		};

		_container = new VContainer(_playButton, _continueButton, exitButton)
		{
			LocalTransform = new Transform()
			{
				Position = new(0f, -30f),
			},
			UniformSize = true
		};
		AddChild(_container);
	}

	public void SetContinueButtonEnabled(bool enabled)
	{
		_continueButton.Enabled = enabled;
	}

	public void NewGameInstead(bool enabled)
	{
		if (enabled)
		{
			_playButton.Text = "Try again";
		}
		else
		{
			_playButton.Text = "Run";
		}
	}

	protected override void OnRender()
	{
		_engine.Renderer.SetCamera(GameRoot.I.CurrentCamera!);
	}
}