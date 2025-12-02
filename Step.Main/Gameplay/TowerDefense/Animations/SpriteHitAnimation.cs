using Silk.NET.Maths;
using Step.Engine.Graphics;
using Step.Engine.Tween;
using Step.Main.Gameplay;

namespace Step.Main.Gameplay.TowerDefense.Animations;

public sealed class SpriteHitAnimation(
	Sprite2d sprite,
	Vector2f baseScale,
	Vector4f baseColor,
	TweenPlayer tweenPlayer,
	float scaleFactor,
	float duration,
	Vector4f? hitColor = null)
{
	private readonly Sprite2d _sprite = sprite ?? throw new ArgumentNullException(nameof(sprite));
	private readonly TweenPlayer _tweenPlayer = tweenPlayer ?? throw new ArgumentNullException(nameof(tweenPlayer));
	private readonly Vector4f _hitColor = hitColor ?? Constants.GameColors.Red;
	private readonly float _scaleFactor = MathF.Max(1f, scaleFactor);
	private readonly float _duration = MathF.Max(0.01f, duration);

	private ITween? _activeTween;

	public void Play()
	{
		ResetState();

		var tweenUp = new FloatTween(
			1f,
			_scaleFactor,
			_duration,
			value =>
			{
				_sprite.LocalTransform.Scale = baseScale * value;
				float factor = CalculateFactor(value);
				_sprite.Color = Vector4D.Lerp(baseColor, _hitColor, factor);
			},
			Easing.EaseOutBack);

		var tweenDown = new FloatTween(
			_scaleFactor,
			1f,
			_duration,
			value =>
			{
				_sprite.LocalTransform.Scale = baseScale * value;
				float factor = CalculateFactor(value);
				_sprite.Color = Vector4D.Lerp(_hitColor, baseColor, 1f - factor);
			},
			Easing.EaseOutQuad);

		_activeTween = new TweenSequence([tweenUp, tweenDown]);
		_tweenPlayer.Play(_activeTween);
	}

	public void Reset()
	{
		ResetState();
	}

	private void ResetState()
	{
		if (_activeTween != null)
		{
			_tweenPlayer.Stop(_activeTween);
			_activeTween = null;
		}

		_sprite.LocalTransform.Scale = baseScale;
		_sprite.Color = baseColor;
	}

	private float CalculateFactor(float value)
	{
		if (_scaleFactor <= 1f)
			return 0f;

		return Math.Clamp((value - 1f) / (_scaleFactor - 1f), 0f, 1f);
	}
}

