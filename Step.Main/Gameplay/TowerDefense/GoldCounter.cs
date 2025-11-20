using Step.Engine;
using Step.Engine.Graphics;
using Step.Engine.Graphics.UI;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class GoldCounter : GameObject
{
	private readonly TowerEconomy _economy;
	private readonly Label _label;
	private readonly Sprite2d _icon;

	private const float IconSize = 18f;

	public GoldCounter(Renderer renderer, TowerEconomy economy)
		: base(nameof(GoldCounter))
	{
		_economy = economy ?? throw new ArgumentNullException(nameof(economy));

		_icon = new Sprite2d(renderer, Assets.LoadTexture2d("Textures/circle-enemy.png"))
		{
			Layer = 70,
			Color = new Vector4f(0.97f, 0.82f, 0.25f, 1f)
		};
		_icon.LocalTransform.Scale = new Vector2f(IconSize, IconSize);
		AddChild(_icon);

		_label = new Label(renderer, Constants.Font.UiFontPath)
		{
			Layer = 70,
			Color = new Vector4f(0.98f, 0.95f, 0.85f, 1f)
		};
		_label.LocalPosition = new Vector2f(IconSize * 0.75f, -IconSize * 0.25f);
		AddChild(_label);
	}

	protected override void OnStart()
	{
		base.OnStart();

		_economy.GoldChanged += HandleGoldChanged;
		UpdateLabel(_economy.CurrentGold);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		_economy.GoldChanged -= HandleGoldChanged;
	}

	private void HandleGoldChanged(int amount)
	{
		UpdateLabel(amount);
	}

	private void UpdateLabel(int amount)
	{
		_label.Text = amount.ToString();
	}
}

