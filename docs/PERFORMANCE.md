# Performance

## Method
- Unity Profiler, Development Build, 1920×1080 windowed.
- Measure 30 s idle title, 30 s battle idle, and a full turn with animations.
- Record: average frame time, 99th percentile, GC alloc per frame, batches, SetPass,
  UI rebuild count.
- Capture before/after in `docs/media/`.

## Baseline (original, Phase 0)
Not yet captured as a player build: the original scenes fail to compile in Unity 6
until T-004, and the battle has no stable turn loop. Static findings:
- `DeckManager.Update()` destroys and instantiates the full card grid every frame.
- `OpponentAttackManager` blocks the main thread for 0.5 s per enemy card.
- `CardDisplay.UpdateVisual()` calls `Resources.Load` indirectly and sets colors every
  click; no pooling.
- UI canvas is one large canvas with per-frame rebuilds on the deck screen.
Target to measure at the first stable build.

## Budgets
| Metric | Budget |
|---|---|
| Frame time at 1080p | ≤ 8.0 ms (125 fps cap) |
| GC alloc per frame in battle | 0 B steady state |
| UI draw batches | ≤ 35 |
| Live card views | ≤ 24 |
| Live particles | ≤ 160 |
| Scene load / boot | ≤ 2.0 s to interactive title |

## After
Filled per phase.
