using OpenTK.Mathematics;

namespace Step.Main;

public class Camera2d
{
	private Matrix4 proj;
	private Vector2 position;
	private Vector2 shakeOffset;

	public Matrix4 ViewProj { get; private set; }

	private static readonly Random Random = new(244554);
	private float _zoom = 1;
	private float shakeDuration = 0f;
	private float shakeMagnitude = 0f;

	public Camera2d(int width, int height)
	{
		proj = Matrix4.CreateOrthographicOffCenter(
			-width / 2f, width / 2f,
			-height / 2f, height / 2f,
			-1f, 100f
		);

		position = Vector2.Zero;
		shakeOffset = Vector2.Zero;
		UpdateViewProj();
	}

	public void Update(float dt)
	{
		UpdateShake(dt);
		UpdateViewProj();
	}

	public void Shake(float magnitude, float duration)
	{
		shakeMagnitude = magnitude;
		shakeDuration = duration;
	}

	private void UpdateShake(float dt)
	{
		if (shakeDuration > 0f)
		{
			shakeOffset.X = (float)(Random.NextDouble() * 2 - 1) * shakeMagnitude;
			shakeOffset.Y = (float)(Random.NextDouble() * 2 - 1) * shakeMagnitude;

			shakeMagnitude *= 0.9f;
			shakeDuration -= dt;

			if (shakeDuration <= 0f)
			{
				shakeDuration = 0f;
				shakeMagnitude = 0f;
				shakeOffset = Vector2.Zero;
			}
		}
	}

	public void Zoom(float scale)
	{
		_zoom += scale;
		_zoom = Math.Clamp(_zoom, 0.1f, 3f);
	}

	private void UpdateViewProj()
	{
		var view = Matrix4.CreateTranslation(
			-(position.X + shakeOffset.X),
			-(position.Y + shakeOffset.Y),
			0f
		) * Matrix4.CreateScale(_zoom);

		ViewProj = view * proj;
	}
}
