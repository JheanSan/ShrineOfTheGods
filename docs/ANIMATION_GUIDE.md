# Animation Guide

All motion runs on unscaled time for UI and scaled time for gameplay. Tweens use
DOTween (`DG.Tweening`) with the curves below. Never `Lerp` linearly.

## Durations (seconds)

| Action | Duration | Ease / curve |
|---|---|---|
| Button hover in | 0.16 | OutQuad, scale 1.00 → 1.035 |
| Button hover out | 0.22 | OutCubic, scale → 1.00 |
| Button press | 0.08 | InQuad, scale → 0.965 |
| Button release | 0.24 | OutBack (overshoot 1.6), scale → 1.00 |
| Screen enter | 0.42 | OutCubic, alpha 0→1 + y −24→0 |
| Screen exit | 0.24 | InCubic, alpha 1→0 + y 0→+12 |
| Modal open | 0.36 | OutBack 1.25, scale 0.92→1 |
| Modal close | 0.20 | InCubic, scale 1→0.94 |
| Card draw | 0.38 | OutCubic, from deck to hand, +12° rotation settle |
| Card play | 0.30 | OutCubic, arc via waypoint, scale 1→1.08→1 |
| Card attack lunge | 0.12 out / 0.18 back | OutQuad / OutCubic |
| Damage number | 0.55 | float 48 px, fade after 0.25 |
| Health bar | 0.35 | OutCubic |
| Turn banner | 0.90 total | in 0.25 OutBack, hold 0.35, out 0.30 InCubic |
| Favour pip fill | 0.30 | OutBack 1.4 |
| Shrine pulse | 0.45 | OutQuad, glow 0→1→0.35 |

## Hit stop
- Normal hit: **0.055 s** time scale 0.
- Critical / blessing: **0.09 s**.
- Game over: 0.16 s, then slow-motion 0.25× for 0.5 s.
Implement with a coroutine that restores `Time.timeScale`; never leave it at 0.

## Screen shake
Decaying positional shake on the UI root, not the camera (game is UI-space).
- Light (chip damage): 3 px, 0.12 s, 18 Hz.
- Medium (card death): 7 px, 0.22 s, 22 Hz.
- Heavy (blessing, defeat): 12 px, 0.35 s, 26 Hz.
Use a decaying sine, random phase per axis, no post-shake offset. Honour the
"Reduce Motion" setting: scale amplitude to 0 and hit stop to 0.

## Squash and stretch
- Card lands: scale (1.06, 0.94) for 0.06 s → (0.98, 1.02) → 1.0.
- Avatar hit: scale (1.05, 0.95), flash bone white 0.06 s.
- Idle: breathing 1.00 → 1.012, 3.6 s, InOutSine, yoyo.
Never squash text independently; apply to the framed container only.

## Anticipation / impact / follow-through
Every powerful action: wind-up opposite the motion (0.10–0.18 s), fast impact,
overshoot, then settle with 1–2 smaller oscillations. No single-frame teleports.

## Particles
- Embers: 0.8–2.4 s life, 6–14 px/s upward, ember gold → transparent.
- Divine motes: cyan, 3–8 px, slow orbit, alpha 0.35–0.7.
- Impact sparks: 8–14 particles, 0.2–0.45 s, gravity-less, fade.
- Blessing: 24 radial gold/cyan glyphs, 0.6 s, OutCubic.
- Max 120 live UI particles; pool them.

## Audio sync
Damage SFX starts on the impact frame (after anticipation), not on click.
UI hover sounds at −18 dB, click at −12 dB; never stack more than 2 at once.

## Reduced motion
When `Settings.ReducedMotion` is on: no shake, no hit stop, durations ×0.6,
fades only, no scale overshoot, particle count ×0.35.
