# Progress

## Active phase
**Phase 6 — README and release** (all implementation phases complete).

## Last completed task
T-037 — README rewritten with screenshots, controls, build/test commands and
architecture summary.

## Next three tasks
1. T-038 — Final QA, tag all phases, merge to develop and push.
2. Optional — native AudioMixer asset and sustained-battle profiling.
3. Optional — physical gamepad verification.

## Open blockers
- None. Unity 6000.3.22f1, licence and Windows build module work; Windows player builds.
- T-019 (gamepad parity) was dropped: EventSystem navigation is wired but no
  physical controller was available for verification.
- A native `AudioMixer` asset remains a roadmap item; routing uses three
  gain-controlled AudioSources.

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
- EditMode: **11/11 passed**. PlayMode: **2/2 passed**.
- Windows player: **build succeeds**; title, covenant, battle, card play, tooltip,
  affordable opening hand and AI turn observed visually in `docs/media/`.
- Performance: **16.66 ms/frame at 60 Hz vsync, 0 B/frame** steady state.
