using Step.Engine;
using Step.Engine.Graphics;
using Step.Engine.Graphics.UI;

namespace Step.Main.Gameplay.UI;

public class GameHud(Renderer renderer, GameInfo gameInfo) : GameObject("GameHud")
{
	private Label? _statCircleLabel;
	private Label? _statCrossLabel;
	private Label? _gliderCrossLabel;

	protected override void OnStart()
	{
		_statCircleLabel = new Label(renderer)
		{
			Text = $"{gameInfo.Coins[Coin.Circle]}",
			FontPath = "Assets/Fonts/Pixellari.ttf",
			FontSize = 16f,
			Color = GameColors.Circle,
		};

		_statCrossLabel = new Label(renderer)
		{
			Text = gameInfo.Coins[Coin.Cross].ToString(),
			FontPath = "Assets/Fonts/Pixellari.ttf",
			FontSize = 16f,
			Color = GameColors.Cross,
		};

		_gliderCrossLabel = new Label(renderer)
		{
			Text = gameInfo.Coins[Coin.Glider].ToString(),
			FontPath = "Assets/Fonts/Pixellari.ttf",
			FontSize = 16f,
			Color = GameColors.Glider,
		};

		var r1 = new TextureRect(renderer)
		{
			Color = GameColors.Glider,
			LocalTransform = new Transform()
			{
				Scale = new(0.8f)
			}
		};
		r1.SetTexture(Assets.LoadTexture2d("Textures/glider-enemy.png"));

		var r2 = new TextureRect(renderer)
		{
			Color = GameColors.Cross,
			LocalTransform = new Transform()
			{
				Scale = new(0.8f)
			}
		};
		r2.SetTexture(Assets.LoadTexture2d("Textures/cross-enemy.png"));

		var r3 = new TextureRect(renderer)
		{
			Color = GameColors.Circle,
			Type = GeometryType.Circle,
			LocalTransform = new Transform()
			{
				Scale = new(0.8f)
			}
		};
		r3.SetTexture(Assets.LoadTexture2d("Textures/circle-enemy.png"));

		AddChild(new VContainer(
			new HContainer(r1, _gliderCrossLabel) { Name = "glider stat" },
			new HContainer(r2, _statCrossLabel) { Name = "cross stat" },
			new HContainer(r3, _statCircleLabel) { Name = "circle stat" })
		{
			LocalTransform = new Transform()
			{
				Position = new Vector2f(-150f, 60f),
				Scale = new(0.5f),
			}
		});
	}

	protected override void OnUpdate(float deltaTime)
	{
		_gliderCrossLabel!.Text = $"{gameInfo.Coins[Coin.Glider]}";
		_statCrossLabel!.Text = $"{gameInfo.Coins[Coin.Cross]}";
		_statCircleLabel!.Text = $"{gameInfo.Coins[Coin.Circle]}";
	}
}