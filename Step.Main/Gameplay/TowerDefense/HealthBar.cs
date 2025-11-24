using Step.Engine;
using Step.Engine.Graphics;
using Step.Engine.Graphics.UI;
using Step.Main.Gameplay.TowerDefense.Core;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class HealthBar : GameObject
{
	private readonly Health _health;
	private readonly Sprite2d _background;
	private readonly Sprite2d _fill;
	private readonly Label _label;
	private readonly float _width;
	private readonly float _height;

	public float Hp => _health.CurrentHealth;

	public HealthBar(
		Renderer renderer,
		float maxHealth,
		float width,
		float height,
		Vector2f localOffset,
		int baseLayer = 0)
		: base(nameof(HealthBar))
	{
		ArgumentNullException.ThrowIfNull(renderer);

		_health = new Health(maxHealth);
		_width = width;
		_height = height;

		LocalTransform.Position = localOffset;

		_background = new Sprite2d(renderer)
		{
			Layer = baseLayer,
			Color = new Vector4f(0f, 0f, 0f, 0.6f)
		};
		_background.LocalTransform.Scale = new Vector2f(_width, _height);
		AddChild(_background);

		_fill = new Sprite2d(renderer)
		{
			Layer = baseLayer + 1,
			Color = new Vector4f(0.2f, 0.8f, 0.2f, 1f),
			Pivot = new Vector2f(0f, 0.5f)
		};
		_fill.LocalTransform.Position = new Vector2f(-_width * 0.5f, 0f);
		_fill.LocalTransform.Scale = new Vector2f(_width, _height);
		AddChild(_fill);

		_label = new Label(renderer, Constants.Font.UiFontPath, 14)
		{
			Layer = baseLayer + 2,
			Color = new Vector4f(0.98f, 0.96f, 0.82f, 1f),
			Pivot = new Vector2f(0.5f, 0.5f)
		};
		AddChild(_label);

		_health.HealthChanged += HandleHealthChanged;
		HandleHealthChanged(_health.CurrentHealth, _health.MaxHealth);
	}

	public void ApplyDamage(float amount)
	{
		_health.ApplyDamage(amount);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		_health.HealthChanged -= HandleHealthChanged;
	}

	private void HandleHealthChanged(float current, float max)
	{
		float ratio = max <= 0f ? 0f : current / max;
		UpdateFill(ratio);
		UpdateLabel(current, max);
	}

	private void UpdateFill(float ratio)
	{
		float clamped = Math.Clamp(ratio, 0f, 1f);
		_fill.Visible = clamped > 0f;
		_fill.LocalTransform.Scale = new Vector2f(_width * clamped, _height);
	}

	private void UpdateLabel(float current, float max)
	{
		int currentValue = (int)MathF.Round(MathF.Max(0f, current));
		int maxValue = (int)MathF.Max(1f, MathF.Round(max));
		_label.Text = $"{currentValue}/{maxValue}";
	}
}

