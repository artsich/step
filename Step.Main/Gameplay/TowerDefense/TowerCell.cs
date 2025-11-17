using Silk.NET.Input;
using Step.Engine;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class TowerCell : GameObject
{
	private readonly Sprite2d _sprite;
	private readonly Input _input;
	private readonly float _cellSize;

	private readonly Vector4f _baseColor = new(0.1f, 0.35f, 0.4f, 0.4f);
	private readonly Vector4f _hoverColor = new(0.35f, 0.85f, 0.9f, 0.7f);
	private readonly Vector4f _occupiedColor = new(0.08f, 0.08f, 0.08f, 0.2f);

	private const float NormalScaleFactor = 0.85f;
	private const float HoverScaleFactor = 0.98f;

	public event Action<TowerCell>? Clicked;

	public Tower? Tower { get; private set; }

	public bool IsOccupied => Tower != null;

	public Vector2f Position => LocalTransform.Position;

	public TowerCell(Renderer renderer, Input input, Vector2f position, float cellSize) 
		: base(nameof(TowerCell))
	{
		_input = input;
		_cellSize = cellSize;
		LocalTransform.Position = position;

		_sprite = new Sprite2d(renderer, renderer.DefaultWhiteTexture)
		{
			Color = _baseColor,
			Layer = 4
		};
		_sprite.LocalTransform.Scale = CreateScaleVector(NormalScaleFactor);

		AddChild(_sprite);
	}

	protected override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);

		if (IsOccupied)
		{
			ApplyOccupiedState();
			return;
		}

		var mousePos = _input.MouseWorldPosition;
		bool hovered = IsMouseOverCell(mousePos);

		if (hovered)
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

	private bool IsMouseOverCell(Vector2f mousePos)
	{
		float half = _cellSize * 0.5f;
		return mousePos.X >= Position.X - half && mousePos.X <= Position.X + half
			&& mousePos.Y >= Position.Y - half && mousePos.Y <= Position.Y + half;
	}

	private void ApplyHoverState()
	{
		_sprite.Color = _hoverColor;
		_sprite.LocalTransform.Scale = CreateScaleVector(HoverScaleFactor);
	}

	private void ApplyIdleState()
	{
		_sprite.Color = _baseColor;
		_sprite.LocalTransform.Scale = CreateScaleVector(NormalScaleFactor);
	}

	private void ApplyOccupiedState()
	{
		_sprite.Color = _occupiedColor;
		_sprite.Visible = false;
	}

	private Vector2f CreateScaleVector(float factor)
	{
		float size = _cellSize * factor;
		return new Vector2f(size, size);
	}
}

