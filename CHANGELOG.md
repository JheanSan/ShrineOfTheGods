# Changelog

All notable changes to *Shrine of the Gods*, newest first.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Phase 5 — Code quality and tests (in progress)
#### Added
- EditMode rule tests (10) and PlayMode boot tests (2); all passing.
- Assembly definitions for runtime, editor and tests; namespaces and pure model.
- `.editorconfig` and an improved `.gitignore`.

#### Fixed
- Earth's first-hit shield now blocks damage before it is applied, both for hero
  strikes and card combat.

### Phase 4 — Original features
#### Added
- Divine Favour, four patron blessings, six round omens and offerings at the shrine.
- Rival hand is hidden and shown as a count.

### Phase 3 — Visuals and audio
#### Added
- Animated painted backdrops, procedural altar, element sigils, gilded frames and
  favour rays.
- Runtime-synthesised ambient music and ten UI/gameplay cues.

### Phase 2 — UI and UX
#### Added
- Title, covenant, battle, chronicle, settings, pause, game-over, tooltip and
  confirmation screens built from a shared UI kit.
- Local Wanderer profile; plaintext login removed with one-time migration.

#### Fixed
- Buttons use procedural frames instead of a sprite with baked "MAIN MENU" text.
- Card views have raycast targets so hover, tooltips, clicks and drags work.
- Hand and field layout no longer clips at 1600×900.

### Phase 1 — Animation and game feel
#### Added
- Pure C# battle model, deterministic AI, turn state machine, card animations,
  hit stop, decaying shake, damage numbers and pooled screen-space motes.

### Phase 0 — Recon and foundation
#### Added
- Full documentation system: AGENTS, style bible, animation and UI guides,
  architecture, testing, performance, decisions, bugs, tasks and progress.
- Standard Unity `.gitignore`; Git LFS reserved for new large binaries.
- `com.unity.ugui` 2.0.0 and `com.unity.test-framework` 1.6.0 packages.
- Repo-hygiene commit that stops tracking generated folders.

#### Changed
- Project upgraded from Unity 2022.3.44f1 to Unity 6000.3.22f1 at the user's request.
- TMP Essential Resources re-imported for uGUI 2.0.
- Removed the broken TextMesh Pro examples/documentation folder from the project;
  it failed to compile under Unity 6 and was never used by gameplay.

#### Fixed
- Malformed `.gitattributes` that marked every word as an LFS filter.
- `OpponentAttackManager` unused-field warning.
- 2,985 `Library/` files and IDE state no longer pollute version control.

#### Removed
- TextMesh Pro example scripts and 2016 PDF guide (third-party samples, compile
  errors under Unity 6, not referenced by the game).
