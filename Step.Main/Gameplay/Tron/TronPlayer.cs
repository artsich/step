using Silk.NET.Input;
using Step.Engine;
using Step.Engine.Collisions;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.Tron;

public sealed class TronPlayer : GameObject
{
	private readonly Renderer _renderer;
	private readonly Input _input;
	private readonly RectangleShape2d _headShape;

	private readonly List<TrailSegment> _trail = [];

	private Vector2f _direction = new(1f, 0f);
	private Vector2f _pendingDirection = new(1f, 0f);
	private TrailSegment? _currentSegment;

	private readonly float _speed;
	private readonly float _size;
	private readonly float _maxTrailLength;

	public event Action? OnDeath;

	public TronPlayer(Renderer renderer, Input input, float size = 4f, float speed = 60f, float maxTrailLength = 200f)
	{
		Name = nameof(TronPlayer);
		_renderer = renderer;
		_input = input;
		_size = size;
		_speed = speed;
		_maxTrailLength = MathF.Max(0f, maxTrailLength);

		_headShape = new RectangleShape2d(renderer)
		{
			Size = new Vector2f(size, size),
			Visible = false,
			IsStatic = false,
			CollisionLayers = (int)PhysicLayers.Player,
			CollisionMask = (int)(PhysicLayers.Frame | PhysicLayers.Trail),
		};
		_headShape.OnCollision += OnHeadCollision;
		AddChild(_headShape);
	}

	protected override void OnUpdate(float dt)
	{
		if (_currentSegment == null)
		{
			StartNewSegment();
		}
		ReadInput();
		if (IsOpposite(_direction, _pendingDirection) == false)
		{
			if (!ApproximatelyEqual(_direction, _pendingDirection))
			{
				_direction = _pendingDirection;
				StartNewSegment();
			}
		}

		GlobalPosition += _direction * _speed * dt;
		ExtendCurrentSegment();
		EnforceMaxTrailLength();
	}

	protected override void OnRender()
	{
		// head
		_renderer.DrawRect(GlobalPosition, new Vector2f(_size, _size), Constants.GameColors.Player, layer: 2);
	}

	private void ReadInput()
	{
		if (_input.IsKeyPressed(Key.W))
		{
			_pendingDirection = new Vector2f(0f, 1f);
		}
		else if (_input.IsKeyPressed(Key.S))
		{
			_pendingDirection = new Vector2f(0f, -1f);
		}
		else if (_input.IsKeyPressed(Key.A))
		{
			_pendingDirection = new Vector2f(-1f, 0f);
		}
		else if (_input.IsKeyPressed(Key.D))
		{
			_pendingDirection = new Vector2f(1f, 0f);
		}
	}

	private void StartNewSegment()
	{
		var seg = new TrailSegment(_renderer, _size)
		{
			Color = Constants.GameColors.Glider,
			Name = $"Trail_{_trail.Count + 1}",
		};
		seg.BeginAt(GlobalPosition, _direction);

		CallDeferred(() =>
		{
			_parent?.AddChild(seg);
			seg.Start();
			_trail.Add(seg);
			_currentSegment = seg;
		});
	}

	private void ExtendCurrentSegment()
	{
		_currentSegment?.ExtendTo(GlobalPosition);
	}

	private void EnforceMaxTrailLength()
	{
		float total = 0f;
		for (int i = 0; i < _trail.Count; i++)
		{
			total += _trail[i].GetLength();
		}

		if (total <= _maxTrailLength)
		{
			return;
		}

		float excess = total - _maxTrailLength;
		const float epsilon = 0.0001f;
		while (excess > epsilon && _trail.Count > 0)
		{
			var first = _trail[0];
			float used = first.TrimFromStart(excess);
			excess -= used;
			if (first.GetLength() <= epsilon)
			{
				_trail.RemoveAt(0);
				first.QueueFree();
			}
		}
	}

	private static bool IsOpposite(in Vector2f a, in Vector2f b)
	{
		return a.X == -b.X && a.Y == -b.Y;
	}

	private static bool ApproximatelyEqual(in Vector2f a, in Vector2f b)
	{
		return Math.Abs(a.X - b.X) < 0.0001f && Math.Abs(a.Y - b.Y) < 0.0001f;
	}

	private void OnHeadCollision(CollisionShape other, CollisionInfo _)
	{
		OnDeath?.Invoke();
	}
}


