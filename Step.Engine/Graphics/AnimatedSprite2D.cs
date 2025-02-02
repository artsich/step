using Serilog;

namespace Step.Engine.Graphics;

public sealed class AnimatedSprite2d(
	Renderer renderer,
	SpriteFrames[] animations) : CanvasItem
{
	private SpriteFrames? _animation;

	public void Play(string name)
	{
		if (_animation?.Name == name) return;

		_animation = animations.FirstOrDefault(x => x.Name == name);

		if (_animation == null)
		{
			Log.Logger.Error($"Animation with name '{name}' not found.");
			_animation = animations.FirstOrDefault();
			if (_animation == null)
			{
				Log.Logger.Error("Default animation not found.");
				return;
			}
		}

		ResetAnimationState();
	}

	protected override void OnUpdate(float dt)
	{
		_animation?.Update(dt);
	}

	protected override void OnRender()
	{
		if (_animation != null)
		{
			var frame = _animation.GetCurrentRect();
			var atlas = _animation.Atlas;

			var model = GetGlobalMatrix();
			renderer.SubmitCommand(new RenderCmd()
			{
				ModelMatrix = model,
				Atlas = atlas,
				AtlasRect = frame,
				Layer = Layer,
				Color = Color,
			});
		}
	}

	private void ResetAnimationState()
	{
		var frame = _animation!.GetCurrentRect();
		LocalTransform.Scale = new(frame.Width, frame.Height);
		_animation?.Reset();
	}
}
