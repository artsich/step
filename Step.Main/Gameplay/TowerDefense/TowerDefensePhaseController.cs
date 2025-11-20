using Step.Engine;
using Step.Engine.Graphics;
using Step.Engine.Graphics.UI;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class TowerDefensePhaseController : GameObject
{
	private readonly Spawns _spawns;
	private readonly Towers _towers;
	private readonly Base _base;
	private readonly FightButton _fightButton;
	private readonly Label _planningLabel;
	private const string PlanningHintText = "Plan your towers";

	private TowerDefensePhase _currentPhase = TowerDefensePhase.Planning;

	public TowerDefensePhaseController(
		Renderer renderer,
		Input input,
		Spawns spawns,
		Towers towers,
		Base baseObject)
		: base(nameof(TowerDefensePhaseController))
	{
		_spawns = spawns;
		_towers = towers;
		_base = baseObject;

		_fightButton = new FightButton(renderer, input, HandleFightPressed);
		_fightButton.LocalPosition = new Vector2f(120f, -75f);
		AddChild(_fightButton);

		_planningLabel = new Label(renderer, Constants.Font.UiFontPath)
		{
			Text = PlanningHintText,
			Layer = 60,
			Visible = false,
			Color = new Vector4f(0.95f, 0.95f, 0.95f, 1f)
		};
		_planningLabel.LocalPosition = new Vector2f(-150f, 65f);
		AddChild(_planningLabel);
	}

	protected override void OnStart()
	{
		base.OnStart();
		_spawns.WaveCompleted += HandleWaveCompleted;
		_base.Dead += HandleBaseDestroyed;
		EnterPlanningPhase();
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		_spawns.WaveCompleted -= HandleWaveCompleted;
		_base.Dead -= HandleBaseDestroyed;
	}

	private void HandleFightPressed()
	{
		if (_currentPhase != TowerDefensePhase.Planning)
			return;

		EnterCombatPhase();
	}

	private void HandleWaveCompleted()
	{
		if (_currentPhase != TowerDefensePhase.Combat)
			return;

		EnterPlanningPhase();
	}

	private void HandleBaseDestroyed()
	{
		EnterGameOverPhase();
	}

	private void EnterPlanningPhase()
	{
		_currentPhase = TowerDefensePhase.Planning;
		_spawns.StopWave();
		_fightButton.Enabled = true;
		_towers.SetPlacementEnabled(true);
		ShowPlanningHint(true);
	}

	private void EnterCombatPhase()
	{
		_currentPhase = TowerDefensePhase.Combat;
		_fightButton.Enabled = false;
		_towers.SetPlacementEnabled(false);
		_spawns.StartWave();
		ShowPlanningHint(false);
	}

	private void EnterGameOverPhase()
	{
		if (_currentPhase == TowerDefensePhase.GameOver)
			return;

		_currentPhase = TowerDefensePhase.GameOver;
		_fightButton.Enabled = false;
		_towers.SetPlacementEnabled(false);
		_spawns.StopWave();
		ShowPlanningHint(false);
	}

	private void ShowPlanningHint(bool visible)
	{
		_planningLabel.Visible = visible;
	}
}

