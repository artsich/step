using Step.Engine;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class TowerDefensePhaseController : GameObject
{
	private readonly Spawns _spawns;
	private readonly Towers _towers;
	private readonly Base _base;
	private readonly FightButton _fightButton;

	private TowerDefensePhase _currentPhase = TowerDefensePhase.Planning;

	public TowerDefensePhaseController(
		Renderer renderer,
		Input input,
		Spawns spawns,
		Towers towers,
		Base baseObject,
		Vector2f uiPosition)
		: base(nameof(TowerDefensePhaseController))
	{
		_spawns = spawns;
		_towers = towers;
		_base = baseObject;

		_fightButton = new FightButton(renderer, input, HandleFightPressed);
		_fightButton.LocalPosition = uiPosition;
		AddChild(_fightButton);
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
	}

	private void EnterCombatPhase()
	{
		_currentPhase = TowerDefensePhase.Combat;
		_fightButton.Enabled = false;
		_towers.SetPlacementEnabled(false);
		_spawns.StartWave();
	}

	private void EnterGameOverPhase()
	{
		if (_currentPhase == TowerDefensePhase.GameOver)
			return;

		_currentPhase = TowerDefensePhase.GameOver;
		_fightButton.Enabled = false;
		_towers.SetPlacementEnabled(false);
		_spawns.StopWave();
	}
}

