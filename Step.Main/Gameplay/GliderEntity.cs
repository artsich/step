using OpenTK.Mathematics;
using Step.Engine;
using Step.Engine.Editor;

namespace Step.Main.Gameplay;

public class GliderEntity(ITarget target)
	: GameObject(nameof(GliderEntity))
{
	[EditorProperty]
	public float Speed { get; set; } = 30f;

	public float MinFollowDistance { get; set; } = 25f;

	private bool _isFollowing = true;
	private Vector2 _constantDir;

	protected override void OnUpdate(float deltaTime)
	{
		var pos = LocalTransform.Position;

		if (_isFollowing)
		{
			var dir = (target.Position - pos);
			var len = dir.Length;
			dir /= len;

			if (len > MinFollowDistance)
			{
				Follow(deltaTime, pos, dir);
			}
			else
			{
				_isFollowing = false;
				_constantDir = dir;
			}
		}
		else
		{

			pos += _constantDir * Speed * deltaTime;
			LocalTransform.Position = pos;
		}
	}

	private void Follow(float deltaTime, Vector2 pos, Vector2 dir)
	{
		pos += dir * Speed * deltaTime;
		LocalTransform.Position = pos;

		var targetAngle = MathF.Atan2(dir.Y, dir.X) - MathF.PI / 2;
		LocalTransform.Rotation = targetAngle;
	}
}
