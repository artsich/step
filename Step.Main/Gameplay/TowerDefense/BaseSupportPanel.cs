using Step.Engine;
using Step.Engine.Graphics;
using Step.Engine.Graphics.UI;

namespace Step.Main.Gameplay.TowerDefense;

public readonly record struct BaseSupportSettings(
	int UpgradeCost,
	int HealCost,
	float UpgradeHealthBonus,
	float HealAmount)
{
	public static BaseSupportSettings Default => new(90, 45, 25f, 30f);
}

public sealed class BaseSupportPanel : GameObject
{
	private const float ButtonWidth = 48f;
	private const float ButtonHeight = 26f;
	private const float HorizontalOffset = 40f;
	private const float CostLabelOffsetY = ButtonHeight * 0.5f + 6f;

	private static readonly Vector4f EnabledColor = new(0.95f, 0.95f, 0.95f, 1f);
	private static readonly Vector4f DisabledColor = new(0.55f, 0.55f, 0.55f, 1f);

	private readonly TowerEconomy _economy;
	private readonly Base _base;
	private readonly BaseSupportSettings _settings;
	private readonly Button _upgradeButton;
	private readonly Button _healButton;
	private readonly Label _upgradeCostLabel;
	private readonly Label _healCostLabel;

	private bool _planningActive;

	public BaseSupportPanel(
		Renderer renderer,
		Input input,
		TowerEconomy economy,
		Base baseObject,
		BaseSupportSettings? settings = null)
		: base(nameof(BaseSupportPanel))
	{
		ArgumentNullException.ThrowIfNull(renderer);
		ArgumentNullException.ThrowIfNull(input);
		_economy = economy ?? throw new ArgumentNullException(nameof(economy));
		_base = baseObject ?? throw new ArgumentNullException(nameof(baseObject));
		_settings = settings ?? BaseSupportSettings.Default;

		LocalTransform.Position = Vector2f.Zero;

		var leftButtonPosition = new Vector2f(-HorizontalOffset, -10f);
		var rightButtonPosition = new Vector2f(HorizontalOffset, -10f);

		_upgradeButton = CreateButton(renderer, input, "Up", leftButtonPosition, HandleUpgradePressed);
		_upgradeCostLabel = CreateCostLabel(
			renderer,
			$"-{_settings.UpgradeCost}g",
			leftButtonPosition + new Vector2f(0f, CostLabelOffsetY));

		_healButton = CreateButton(renderer, input, "Hp", rightButtonPosition, HandleHealPressed);
		_healCostLabel = CreateCostLabel(
			renderer,
			$"-{_settings.HealCost}g",
			rightButtonPosition + new Vector2f(0f, CostLabelOffsetY));

		SetPlanningMode(false);
	}

	public void SetPlanningMode(bool planningActive)
	{
		_planningActive = planningActive;
		bool shouldShow = planningActive && !_base.IsDestroyed;
		Enabled = shouldShow;
		UpdateButtonsState();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_economy.GoldChanged += HandleGoldChanged;
		_base.Dead += HandleBaseDestroyed;
		UpdateButtonsState();
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		_economy.GoldChanged -= HandleGoldChanged;
		_base.Dead -= HandleBaseDestroyed;
	}

	private Button CreateButton(Renderer renderer, Input input, string text, Vector2f localPosition, Action onClick)
	{
		var button = new Button(text, Constants.Font.UiFontPath, onClick, input, renderer)
		{
			TextAlignment = TextAlignment.Center,
			Layer = 55
		};
		button.Size = new Vector2f(ButtonWidth, ButtonHeight);
		button.TextColor = new Vector4f(0.1f, 0.1f, 0.1f, 1f);
		button.LocalPosition = localPosition;

		AddChild(button);
		return button;
	}

	private Label CreateCostLabel(Renderer renderer, string text, Vector2f localPosition)
	{
		var label = new Label(renderer, Constants.Font.UiFontPath, 12)
		{
			Text = text,
			Layer = 54,
			Color = EnabledColor,
			Pivot = new Vector2f(0.5f, 0f)
		};
		label.LocalPosition = localPosition;
		AddChild(label);
		return label;
	}

	private void HandleUpgradePressed()
	{
		if (!_planningActive || !_base.CanFortify)
			return;

		if (!_economy.TrySpendGold(_settings.UpgradeCost))
			return;

		_base.Fortify(_settings.UpgradeHealthBonus);
		UpdateButtonsState();
	}

	private void HandleHealPressed()
	{
		if (!_planningActive || !_base.NeedsHealing)
			return;

		if (!_economy.TrySpendGold(_settings.HealCost))
			return;

		_base.Heal(_settings.HealAmount);
		UpdateButtonsState();
	}

	private void HandleGoldChanged(int _)
	{
		UpdateButtonsState();
	}

	private void HandleBaseDestroyed()
	{
		SetPlanningMode(false);
	}

	private void UpdateButtonsState()
	{
		bool showButtons = _planningActive && !_base.IsDestroyed;

		SetButtonState(_upgradeButton, _upgradeCostLabel, showButtons && _base.CanFortify, _settings.UpgradeCost);
		SetButtonState(_healButton, _healCostLabel, showButtons && _base.NeedsHealing, _settings.HealCost);
	}

	private void SetButtonState(Button button, Label costLabel, bool actionAvailable, int cost)
	{
		bool visible = _planningActive && !_base.IsDestroyed;
		button.Visible = visible;
		costLabel.Visible = visible;

		bool canAfford = actionAvailable && _economy.CanAfford(cost);
		button.Enabled = canAfford;
		costLabel.Color = canAfford ? EnabledColor : DisabledColor;
	}
}


