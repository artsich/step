using Silk.NET.Input;
using Step.Engine;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class TowerCell : GameObject
{
	private readonly Sprite2d _sprite;
	private readonly Input _input;
	private readonly float _cellSize;

	private readonly Vector4f _hoverColor = new(0.35f, 0.85f, 0.9f, 0.7f);

	private const float NormalScaleFactor = 0.85f;
	private const float HoverScaleFactor = 0.98f;

	private bool _isHovered;

	public event Action<TowerCell>? Clicked;

	public Tower? Tower { get; private set; }

	public bool IsOccupied => Tower != null;

	public Vector2f Position => LocalTransform.Position;

	public bool InteractionEnabled { get; set; } = true;

	public TowerCell(Renderer renderer, Input input, Vector2f position, float cellSize) 
		: base(nameof(TowerCell))
	{
		_input = input;
		_cellSize = cellSize;
		LocalTransform.Position = position;

		_sprite = new Sprite2d(renderer, renderer.DefaultWhiteTexture)
		{
			Layer = 4,
			Visible = false
		};
		_sprite.LocalTransform.Scale = CreateScaleVector(NormalScaleFactor);

		AddChild(_sprite);
	}

	protected override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);

		if (!InteractionEnabled)
		{
			_isHovered = false;
			ApplyDisabledState();
			return;
		}

		if (IsOccupied)
		{
			ApplyOccupiedState();

			if (_isHovered && _input.IsMouseButtonJustPressed(MouseButton.Left))
			{
				Clicked?.Invoke(this);
			}

			return;
		}

		if (_isHovered)
		{
			ApplyHoverState();
			if (_input.IsMouseButtonJustPressed(MouseButton.Left))
			{
				Clicked?.Invoke(this);
			}
		}
		else
		{
			ApplyIdleState();
		}
	}

	public bool TryOccupy(Tower tower)
	{
		if (IsOccupied)
			return false;

		Tower = tower;
		_sprite.Visible = false;
		return true;
	}

	public Tower? ReleaseTower()
	{
		if (Tower == null)
			return null;

		var removedTower = Tower;
		Tower = null;

		if (InteractionEnabled)
		{
			ApplyIdleState();
		}
		else
		{
			ApplyDisabledState();
		}

		return removedTower;
	}

	private bool IsMouseOverCell(Vector2f mousePos)
	{
		float half = _cellSize * 0.5f;
		return mousePos.X >= Position.X - half && mousePos.X <= Position.X + half
			&& mousePos.Y >= Position.Y - half && mousePos.Y <= Position.Y + half;
	}

	protected override void OnEvent(Event e)
	{
		if (e is MouseHoverEvent mouseHoverEvent)
		{
			if (!InteractionEnabled)
			{
				_isHovered = false;
				return;
			}

			var isOver = IsMouseOverCell(mouseHoverEvent.Position);

			if (isOver && !mouseHoverEvent.Handled)
			{
				_isHovered = true;
				mouseHoverEvent.MarkHandled();
			}
			else
			{
				_isHovered = false;
			}
		}

		base.OnEvent(e);
	}

	private void ApplyHoverState()
	{
		_sprite.Visible = true;
		_sprite.Color = _hoverColor;
		_sprite.LocalTransform.Scale = CreateScaleVector(HoverScaleFactor);
	}

	private void ApplyIdleState()
	{
		_sprite.Visible = false;
	}

	private void ApplyOccupiedState()
	{
		_sprite.Visible = false;
	}

	private void ApplyDisabledState()
	{
		_sprite.Visible = false;
	}

	private Vector2f CreateScaleVector(float factor)
	{
		float size = _cellSize * factor;
		return new Vector2f(size, size);
	}
}

