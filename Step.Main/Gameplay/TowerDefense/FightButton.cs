using Step.Engine;
using Step.Engine.Graphics;
using Step.Engine.Graphics.UI;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class FightButton : GameObject
{
	private readonly Button _button;

	public FightButton(Renderer renderer, Input input, Action onClick) : base(nameof(FightButton))
	{
		_button = new Button("Fight", Constants.Font.UiFontPath, onClick, input, renderer)
		{
			TextAlignment = TextAlignment.Center,
			Layer = 50
		};
		_button.Size = new Vector2f(110f, 40f);
		_button.TextColor = new Vector4f(0.1f, 0.1f, 0.1f, 1f);

		AddChild(_button);
	}

	protected override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);
		_button.Visible = Enabled;
		_button.Enabled = Enabled;
	}
}

