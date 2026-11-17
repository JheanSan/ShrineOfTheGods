# Progress

## Active phase
**Phase 5 — Code Quality and Performance** (Phases 1–4 implemented and visually verified).

## Last completed task
T-034 — PlayMode smoke tests pass (2/2); EditMode rules tests pass (10/10).

## Next three tasks
1. T-032 — Pool card views and remove remaining card create/destroy churn.
2. T-035 — Profile the title and battle, record frame time and GC in PERFORMANCE.md.
3. T-037 — Rewrite README.md with final screenshots and honest status.

## Open blockers
- None. Unity 6000.3.22f1, licence and Windows build module work; Windows player builds.
- Optional: `AudioMixer` is currently emulated by three AudioSources with gain
  groups; a real mixer asset is still needed for T-023.
- Gamepad navigation is not fully verified (T-019).

## Phase reports

### Phase 0
Confirmed the original: Unity 2022.3.44f1, built-in RP, six uGUI scenes, plaintext
login, deck builder, simple card battle. Visual baseline captured; the original
battle showed a raw yellow/blue placeholder and crashed with an empty deck.
Upgraded to Unity 6000.3.22f1 and documented everything.

### Phase 1–2
Rebuilt the game as a single scene with a pure C# battle model and a runtime UI.
The title, covenant, battle and overlay screens were verified in the built player
at 1600×900. Fixed invisible procedural frames, baked button text, missing card
raycast targets, clipped hand cards and invisible overlays. The battle now has
impact flashes, decaying shake, hit stop, damage numbers, pooled motes, card arcs
and an affordable opening hand. `ANIMATION_GUIDE.md` holds the standard values.

### Phase 3–4
Painted backgrounds animate with slow zoom/drift; procedural altar, sigils, glowing
favour rays and gilded frames carry the original style. All sound is synthesised
at runtime (no unlicensed samples). Added divine favour, four patron blessings,
six round omens, offerings and a hidden rival hand. Each feature is documented in
DECISIONS.md and surfaced in the HUD and tooltips.

## Verification so far
- Batchmode compile: **zero errors, zero warnings**.
- EditMode: **10/10 passed**. PlayMode: **2/2 passed**.
- Windows player: **build succeeds**; title, covenant, battle, tooltip and AI turn
  observed visually in `docs/media/`.
