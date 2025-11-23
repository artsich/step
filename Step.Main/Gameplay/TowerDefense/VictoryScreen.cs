using Step.Engine;
using Step.Engine.Graphics;
using Step.Engine.Graphics.UI;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class VictoryScreen : GameObject
{
	private readonly Label _victoryLabel;

	public VictoryScreen(Renderer renderer) : base(nameof(VictoryScreen))
	{
		_victoryLabel = new Label(renderer, Constants.Font.UiFontPath)
		{
			Text = "Victory!",
			Layer = 100,
			Color = new Vector4f(0.95f, 0.95f, 0.1f, 1f),
			Visible = false
		};
		_victoryLabel.LocalPosition = new Vector2f(0f, 0f);
		AddChild(_victoryLabel);
	}

	public void Show()
	{
		_victoryLabel.Visible = true;
	}

	public void Hide()
	{
		_victoryLabel.Visible = false;
	}
}

