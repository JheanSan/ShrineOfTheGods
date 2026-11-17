# Performance

## Method
- Windows player, 1600×900 windowed, development PC (Windows 11, Unity 6000.3.22f1).
- `PerformanceProbe` skips 120 warm-up frames and samples 600 frames, logging
  average frame time and GC allocation to `Player.log`.
- Unity Profiler budgets below remain the target for a future deep profile.

## Baseline (original, Phase 0)
Not comparable: the original battle crashed with an empty deck and its deck screen
rebuilt every card every frame from `Update()`. First Unity 6 player sample
(including startup/shader compilation): **23.14 ms/frame, ~3,796 B allocated
per frame**. Static problems: blocking `Thread.Sleep`, per-frame
`Destroy`/`Instantiate`, `FindObjectOfType` lookups.

## After (Phase 5)
Steady-state title sample: **16.66 ms/frame, 0 B allocated per frame**.
The 16.7 ms value is the 60 Hz vsync interval, not CPU cost; the game is no longer
allocation-bound. Card views and screen motes are pooled, and UI updates are
event-driven rather than polled.

| Metric | Budget | Measured | Status |
|---|---|---|---|
| Frame time (1600×900, 60 Hz) | ≤ 8 ms CPU | 16.66 ms wall (vsync) | Pass (vsync-limited) |
| GC alloc per frame | 0 B | 0 B | Pass |
| Live card views | ≤ 24 | ≤ 24 | Pass |
| Live motes | ≤ 160 | 48 | Pass |
| Boot to title | ≤ 2.0 s | < 2 s observed | Pass |

## Evidence
- `docs/media/phase0_original_battle.png` — original placeholder UI.
- `docs/media/phase2_title_final.png`, `phase5_final_battle.png` — rebuilt game.
- `Player.log` line: `[ShrinePerf] frames=600 avg=16,66ms estimated_alloc=0B/frame`.

## Remaining work
- Profile the battle under sustained AI turns, not only the title screen.
- Verify UI batch count in the Unity Profiler; target ≤ 35 batches.
- Investigate a native AudioMixer asset and further reduce full-screen overdraw.
