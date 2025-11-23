# Tower Defense Game – Current Snapshot

## Overview
- Project: `Step.Main` (Silk.NET + custom Step.Engine stack).
- Mode: single-level tower defense prototype focused on core mechanics—tower placement, multiple enemy waves, base defense.
- Entry point: `Program.cs` → `GameScene` (handles menu/game states) → `GameBuilder` (instantiates gameplay objects).

## Gameplay Flow
1. **Main Menu** (`MainMenu`): play / continue / exit. Starting a run builds a fresh `GameLoop`.
2. **Planning Phase** (`TowerDefensePhaseController`):
   - Towers can be placed on predefined cells.
   - Hint text "Plan your towers" is shown in the top-left corner.
   - `Fight` button bottom-right starts combat.
3. **Combat Phase**:
   - Current wave spawns (`Spawns.StartWave`).
   - Towers lock onto nearest enemy within range and fire projectiles automatically.
   - When wave completes, automatically returns to Planning Phase for next wave preparation.
4. **Win Condition**: When all waves are completed, victory screen is shown.
5. **Lose Condition**: Base health reaches zero (`Base.Dead`). Game loop finishes and user is returned to menu.

## Core Systems
- **Level & Pathing** (`Gameplay/TowerDefense/Core`):
  - Hard-coded ASCII map in `GameBuilder.CreateLevel`.
  - Wave configuration: multiple waves with different enemy types, counts, and spawn intervals.
  - Pathfinding precalculates paths from each spawn to the base.
- **Base** (`Base`):
  - 100 HP; each enemy reaching base deals 10 HP damage.
  - Emits `Dead` event when destroyed.
- **Wave System** (`WaveConfig`, `EnemyType`, `EnemyTypeConfig`):
  - Each wave defines enemy types with weights for random selection, total enemy count, and spawn interval.
  - Enemy types: `Enemy1` (2 HP), `Enemy2` (3 HP), `Enemy3` (5 HP).
  - Each enemy type has predefined health, move speed, and color.
  - Waves can mix multiple enemy types using weighted random selection.
- **Spawns & Wave Manager** (`Spawns`):
  - Manages multiple waves sequentially.
  - Automatically transitions to next wave after completion.
  - Responsible for spawn markers, enemy lifecycle, and wave-complete detection.
  - Exposes `ActiveEnemies`, `WaveInProgress`, `WaveCompleted`, `AllWavesCompleted` events.
  - Tracks current wave number and total waves.
- **Enemies** (`Enemy`):
  - Follow path nodes sequentially, moving at constant speed.
  - Support different types with varying health, speed, and visual appearance (color).
  - Emit `ReachedBase` or `Died`; self-cleanup via `QueueFree`.
- **Towers & Cells** (`Towers`, `TowerCell`, `Tower`):
  - Click on highlighted cells during Planning to instantiate a tower.
  - Clicking an occupied cell now sells the tower and refunds its cost.
  - Towers search `Spawns.ActiveEnemies` for nearest target in range and fire projectiles with cooldown.
  - Cell interactions are disabled automatically outside Planning phase.
- **Economy** (`TowerEconomy`, `GoldCounter`):
  - Start with enough gold for one tower (`StartingGold = TowerCost` by default).
  - Each kill awards configurable gold (`GoldPerKill`) that can be tuned via `TowerEconomySettings`.
  - Purchasing a tower withdraws its cost; UI ignores clicks if there isn't enough gold.
  - Current gold is displayed in the top-left next to a gold coin icon for quick feedback.
- **UI Elements**:
  - `FightButton`: enabled only while planning.
  - `GameTimer`: simple session timer centered at top.
  - Planning hint label in top-left, toggled by phase controller.
  - `VictoryScreen`: displayed when all waves are completed.

