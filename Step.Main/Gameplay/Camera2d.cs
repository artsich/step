using ImGuiNET;
using OpenTK.Mathematics;
using Step.Engine;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay;

public class Camera2d : GameObject, ICamera2d
{
	private static readonly Random Random = new(244554);
	private readonly IGameWindow _window;
	private Matrix4 proj;
	private Vector2 shakeOffset;

	public Matrix4 ViewProj { get; private set; }

	private float shakeDuration = 0f;
	private float shakeMagnitude = 0f;

	public Camera2d(float width, float height, IGameWindow window)
		: base(nameof(Camera2d))
	{
		proj = Matrix4.CreateOrthographicOffCenter(
			-width / 2f, width / 2f,
			-height / 2f, height / 2f,
			-1f, 100f
		);
		_window = window;

		LocalTransform.Position = Vector2.Zero;
		UpdateViewProj();
	}

	public Vector2 ScreenToWorld(Vector2 screenPos)
	{
		Vector2 normalizedScreenPos = new(
			screenPos.X / _window.Size.X * 2 - 1,
			-(screenPos.Y / _window.Size.Y * 2 - 1)
		);

		var inverseViewProj = Matrix4.Invert(ViewProj);
		return (new Vector4(normalizedScreenPos, 0f, 1f) * inverseViewProj).Xy;
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

			shakeMagnitude = MathHelper.Lerp(shakeMagnitude, 0.0f, 5f * dt);
			shakeDuration -= dt;

			if (shakeDuration <= 0f)
			{
				shakeDuration = 0f;
				shakeMagnitude = 0f;
				shakeOffset = Vector2.Zero;
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
