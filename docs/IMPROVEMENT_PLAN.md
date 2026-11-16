# Improvement Plan

Written at the end of Phase 0 recon. Priorities: **P0** = must ship, **P1** = strong
value, **P2** = polish if time permits. Every item must pass the Style Bible.

## Diagnosis

The original project is a 2D uGUI card battler:
- Six scenes: `MenuInicial`, `LoginScene`, `MainMenu`, `PlayScene`, `ManageDeckScene`,
  `ViewProfileScene`. Built on Unity 2022.3.44f1, now upgraded to 6000.3.22f1.
- 17 cards defined in code, but only 3 card artworks exist; the rest silently fail.
  `DeckManager` calls `Update()` and rebuilds the whole deck display every frame.
- `OpponentAttackManager.PerformAttacks()` blocks the main thread with `Thread.Sleep`.
- Turn flow is a mix of coroutines, `Invoke` and a random first player; the opponent
  can be skipped or act twice. No turn state machine.
- `UserManager` stores plaintext passwords and fake auth. There is no pause, settings
  persistence, keyboard/gamepad focus, or audio settings that save.
- DOTween is installed but unused. No particles, no shake, no hit stop, no screen flow.
- TMP examples broke the Unity 6 upgrade and were removed (see BUGS.md B-001).

The art is the project's best asset: painted dark-fantasy backgrounds, a gilded
card frame and card back, god portraits, and a strong midnight/ember palette. The
overhaul should reveal that art instead of replacing it.

## Phase 0 — Recon and foundation (P0)
- T-001 Search/clone repo, install/verify environment.
- T-002 Fix repo hygiene: untrack `Library`, `Logs`, `UserSettings`, `.vs`; repair
  `.gitattributes`/`.gitignore`; preserve the duplicate archive in history.
- T-003 Run the project; record Unity version, packages, scenes, loop, weaknesses.
- T-004 Write docs, style bible, plan and backlog; upgrade to Unity 6000.3.22f1.

## Phase 1 — Animation and game feel (P0)
- Replace the battle scene wiring with a deterministic battle state machine.
- Card hand fan, hover lift, drag tilt, play arc, draw, discard and death animations.
- Lunge attacks, impact flash, damage numbers, hit stop, screen shake, squash/stretch.
- Animated HP/mana/favour bars, turn banner, omen reveal, shrine pulse.
- Embers/motes/dust particles in screen space; god avatars breathe and react.
- Standardise all timings in `ANIMATION_GUIDE.md`; reduced-motion support.
- Core-loop fixes: player always starts, AI never blocks, cards on field can attack.

## Phase 2 — UI and UX (P0)
- Rebuild every screen in one scene with runtime UI and consistent components:
  title, deck builder, battle, profile/chronicle, settings, pause, game over.
- Replace the fake login wall with a local Wanderer profile; no plaintext passwords.
- Keyboard and gamepad navigation with visible focus, cancel/back, tooltips.
- Settings that save: volumes, fullscreen, resolution, motion, shake, UI scale.
- Safe areas, 16:9 and 16:10 scaling, 1280×720 minimum readability.
- Animated screen transitions, button states, tooltips and confirmation popups.
- Document components, spacing and navigation in `UI_GUIDE.md`.

## Phase 3 — Visuals and audio (P0/P1)
- Animated parallax background, fog layers, vignette, gold dust.
- Procedural shrine altar, god medallions, element sigils and card frames.
- UI glow/ember shader or layered additive sprites; no render-pipeline switch.
- AudioMixer groups (Master/Music/SFX/UI), music transitions, UI and card SFX,
  pitch/volume variation, ambient shrine loop. All audio CC0/OFL-compatible.
- Environmental storytelling in the background and card lore.
- Before/after screenshots and a short recording in `docs/media/`.

## Phase 4 — Original features (P0)
- **Divine Favour**: build 0–100 by playing favoured cards and offerings; spend a
  full gauge on the patron god's blessing (Flame/Tide/Stone/Gale).
- **Omens**: one omen per round changes a rule and is announced with a banner.
- **Offerings**: sacrifice a card from hand at the shrine for favour and a draw.
- **Curses**: omens can carry a price; the player chooses to accept or pay mana.
- **Patron gods**: three selectable patrons with distinct blessings and avatars,
  woven into deck building and the battle HUD.
- Each mechanic gets a design note in DECISIONS.md, UI affordance, effects, balance
  values in a ScriptableObject and EditMode tests.

## Phase 5 — Code quality and performance (P0)
- `Assets/_Project` layout, asmdefs, namespaces, one class per file.
- Pure C# battle model (no MonoBehaviour), ScriptableObject card/omen definitions.
- Remove singletons and `FindObjectOfType`; explicit composition root.
- Pool cards, damage numbers and particles; no per-frame allocations.
- Cache references; guard nulls; no `Update()` UI rebuilds.
- Editor tests for combat/economy/omen/favour, PlayMode smoke test for a full turn.
- `.editorconfig`, correct `.gitignore`, Git LFS for new large media, no secrets.

## Phase 6 — README and release (P0)
- Rewrite README with title/tagline, GIFs, features, controls, requirements,
  how to open/build, structure, architecture, tech stack, roadmap, credits.
- Final QA: zero errors, zero new warnings, tests pass, full play session clean.
- Tag phases, merge to `develop`, push branches and tags.

## Explicit non-goals
- No render-pipeline switch (UI game; built-in is sufficient).
- No online backend, real money, ads or analytics.
- No replacing the existing painted art with a different style.
- No mobile release in this overhaul; the layout stays resolution-adaptive.
