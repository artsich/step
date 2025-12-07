using Step.Engine;
using Step.Engine.Graphics;
using Step.Engine.Graphics.UI;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class SpeedUpTime : GameObject
{
	private readonly Button _button;
	private bool _isSpeedUp = false;

	public int TimeScale { get; set; } = 4;

	private string TimeScaleText => _isSpeedUp ? "x1" : $"x{TimeScale}";

	public SpeedUpTime(Renderer renderer, Input input) : base(nameof(SpeedUpTime))
	{
		_button = new Button(TimeScaleText, Constants.Font.UiFontPath, ToggleSpeed, input, renderer)
		{
			TextAlignment = TextAlignment.Center,
			Layer = 50,
			Size = new Vector2f(110f, 40f),
			TextColor = new Vector4f(0.1f, 0.1f, 0.1f, 1f)
		};
		AddChild(_button);
	}

	public void Reset()
	{
		_isSpeedUp = false;
		GameRoot.I.TimeScale = 1.0f;
		_button.Text = TimeScaleText;
	}

	private void ToggleSpeed()
	{
		_isSpeedUp = !_isSpeedUp;
		GameRoot.I.TimeScale = _isSpeedUp ? TimeScale : 1.0f;
		_button.Text = TimeScaleText;
	}

	protected override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);
		_button.Visible = Enabled;
		_button.Enabled = Enabled;
	}
}

