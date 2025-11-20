# Tower Defense Game – Current Snapshot

## Overview
- Project: `Step.Main` (Silk.NET + custom Step.Engine stack).
- Mode: single-level tower defense prototype focused on core mechanics—tower placement, one enemy wave, base defense.
- Entry point: `Program.cs` → `GameScene` (handles menu/game states) → `GameBuilder` (instantiates gameplay objects).

## Gameplay Flow
1. **Main Menu** (`MainMenu`): play / continue / exit. Starting a run builds a fresh `GameLoop`.
2. **Planning Phase** (`TowerDefensePhaseController`):
   - Towers can be placed on predefined cells.
   - Hint text “Plan your towers” is shown in the top-left corner.
   - `Fight` button bottom-right starts combat.
3. **Combat Phase**:
   - Selected wave spawns (`Spawns.StartWave`).
   - Towers lock onto nearest enemy within range and fire projectiles automatically.
4. **Win Condition**: When the entire wave is spawned and all enemies are dead, the controller returns to the Planning Phase (no rewards yet; same wave repeats).
5. **Lose Condition**: Base health reaches zero (`Base.Dead`). Game loop finishes and user is returned to menu.

## Core Systems
- **Level & Pathing** (`Gameplay/TowerDefense/Core`):
  - Hard-coded ASCII map in `GameBuilder.CreateLevel`.
  - Spawn config: `10` enemies at `0.5` spawns/sec (single wave reused each loop).
  - Pathfinding precalculates paths from each spawn to the base.
- **Base** (`Base`):
  - 100 HP; each enemy reaching base deals 10 HP damage.
  - Emits `Dead` event when destroyed.
- **Spawns & Wave Manager** (`Spawns`):
  - Responsible for spawn markers, enemy lifecycle, and wave-complete detection.
  - Exposes `ActiveEnemies`, `WaveInProgress`, `WaveCompleted` event.
- **Enemies** (`Enemy`):
  - Follow path nodes sequentially, moving at constant speed.
  - Emit `ReachedBase` or `Died`; self-cleanup via `QueueFree`.
- **Towers & Cells** (`Towers`, `TowerCell`, `Tower`):
  - Click on highlighted cells during Planning to instantiate a tower.
  - Towers search `Spawns.ActiveEnemies` for nearest target in range and fire projectiles with cooldown.
  - Cell interactions are disabled automatically outside Planning phase.
- **UI Elements**:
  - `FightButton`: enabled only while planning.
  - `GameTimer`: simple session timer centered at top.
  - Planning hint label in top-left, toggled by phase controller.

