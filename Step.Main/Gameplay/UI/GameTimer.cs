using Step.Engine;
using Step.Engine.Graphics;
using Step.Engine.Graphics.UI;

namespace Step.Main.Gameplay.UI;

public sealed class GameTimer : GameObject
{
	private readonly Label _label;
	private float _elapsed;

	public GameTimer(Renderer renderer)
	{
		_label = new Label(renderer)
		{
			FontPath = Constants.Font.UiFontPath,
		};
		AddChild(_label);
	}

	protected override void OnUpdate(float deltaTime)
	{
		_elapsed += deltaTime;
		var span = TimeSpan.FromSeconds(_elapsed);

		_label.LocalPosition = _label.LocalPosition with
		{
			X = -_label.Size.X / 2f,
		};

		_label.Text = $"{span.Minutes:D2}.{span.Seconds:D2}";
	}
}
