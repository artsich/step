using ImGuiNET;

namespace Step.Engine.Graphics;

public sealed class Camera2d : GameObject, ICamera2d
{
	private static readonly Random Random = new(244554);
	private Matrix4f proj;
	private Vector2f shakeOffset;

	public Matrix4f ViewProj { get; private set; }

	private float shakeDuration = 0f;
	private float shakeMagnitude = 0f;

	public Camera2d(float width, float height)
		: base(nameof(Camera2d))
	{
		proj = Matrix4.CreateOrthographicOffCenter(
			-width / 2f, width / 2f,
			-height / 2f, height / 2f,
			-1f, 100f
		);

		LocalTransform.Position = Vector2f.Zero;
		UpdateViewProj();
	}

	public void Shake(float magnitude, float duration)
	{
		shakeMagnitude = magnitude;
		shakeDuration = duration;
	}

	public void Zoom(float scale)
	{
		var zoom = LocalTransform.Scale;

		zoom.X = Math.Clamp(zoom.X + scale, 0.1f, 3f);
		zoom.Y = Math.Clamp(zoom.Y + scale, 0.1f, 3f);

		LocalTransform.Scale = zoom;
	}

	protected override void OnDebugDraw()
	{
		ImGui.Text($"Zoom: {LocalTransform.Scale.X}");
	}

	protected override void OnUpdate(float dt)
	{
		UpdateShake(dt);
		UpdateViewProj();
	}

	private void UpdateShake(float dt)
	{
		if (shakeDuration > 0f)
		{
			shakeOffset.X = (float)(Random.NextDouble() * 2 - 1) * shakeMagnitude;
			shakeOffset.Y = (float)(Random.NextDouble() * 2 - 1) * shakeMagnitude;

			shakeMagnitude = float.Lerp(shakeMagnitude, 0.0f, 5f * dt);
			shakeDuration -= dt;

			if (shakeDuration <= 0f)
			{
				shakeDuration = 0f;
				shakeMagnitude = 0f;
				shakeOffset = Vector2f.Zero;
			}
		}
	}

	private void UpdateViewProj()
	{
		var shakeT = Matrix4.CreateTranslation(-shakeOffset.X, -shakeOffset.Y, 0f);
		var view = GetGlobalMatrix() * shakeT;
		ViewProj = view * proj;
	}
}
