using Step.Engine;
using Step.Engine.Graphics;
using Step.Engine.Graphics.UI;

namespace Step.Main.Gameplay.UI;

public class MainMenu : GameObject
{
	private static readonly string FontPath = Constants.Font.UiFontPath;
	private readonly VContainer _container;
	private readonly Button _continueButton;

	public event Action? OnPlayPressed;
	public event Action? OnContinuePressed;
	public event Action? OnExitPressed;

	private readonly Button _playButton;

	public MainMenu(Engine.Engine engine) : base("MainMenu")
	{
		_playButton = new Button("Play", FontPath, () => OnPlayPressed?.Invoke(), engine.Input, engine.Renderer)
		{
			TextColor = Color.Red,
			TextAlignment = TextAlignment.Center
		};

		_continueButton = new Button("Continue", FontPath, () => OnContinuePressed?.Invoke(), engine.Input, engine.Renderer)
		{
			TextColor = Color.Red,
			Enabled = false,
			TextAlignment = TextAlignment.Center
		};

		var exitButton = new Button("Exit", FontPath, () => OnExitPressed?.Invoke(), engine.Input, engine.Renderer)
		{
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
			_playButton.Text = "Restart";
		}
		else
		{
			_playButton.Text = "Run";
		}
	}
}