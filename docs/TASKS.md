# Tasks

One task = one commit. Status: `todo`, `doing`, `done`, `dropped`.
Commit links use the short hash once the task is committed.

| ID | Phase | Title | Status | Commit | Notes |
|---|---|---|---|---|---|
| T-001 | 0 | Locate project, verify Unity install, clone repo | done | — | GitHub `JheanSan/ShrineOfTheGods` cloned to `~/Projects`. |
| T-002 | 0 | Repair git hygiene and protected branches | done | `b263fff` | Untracked Library/Logs/UserSettings/.vs; new .gitignore/.gitattributes. |
| T-003 | 0 | Run original project and document recon | done | — | Unity 2022.3.44f1, built-in RP, uGUI card battler; six scenes. |
| T-004 | 0 | Upgrade to Unity 6000.3.22f1 and document | doing | — | Removed broken TMP examples; added uGUI/test framework. |
| T-005 | 1 | Create `Shrine` scene, bootstrap and composition root | todo | — | Single runtime-built UI; old scenes kept out of build. |
| T-006 | 1 | Pure battle rules model + deterministic AI | todo | — | No MonoBehaviour; events; seeded RNG. |
| T-007 | 1 | Card data as ScriptableObjects, full 17-card pool | todo | — | Values, lore, element, sigil. |
| T-008 | 1 | Card view: frame, art/sigil, hover, drag, play arc | todo | — | Reuse existing card frame/back. |
| T-009 | 1 | Turn state machine + attack with field cards | todo | — | No Invoke, no Thread.Sleep. |
| T-010 | 1 | Impact package: flash, shake, hit stop, damage numbers | todo | — | Values in ANIMATION_GUIDE. |
| T-011 | 1 | Draw/play/death/discard animations | todo | — | DOTween, pooled views. |
| T-012 | 1 | Screen-space embers/motes/dust particles | todo | — | Pooled, reduced-motion aware. |
| T-013 | 2 | UI kit: gilded panel, button, pill, tooltip, modal | todo | — | Runtime components, no scene wiring. |
| T-014 | 2 | Title screen, animated background and screen flow | todo | — | Reuse FondoMenu/Dragonfondo. |
| T-015 | 2 | Deck builder screen with 8-card covenant | todo | — | Element filters, deck summary. |
| T-016 | 2 | Battle HUD: HP, mana, favour, omen, shrine | todo | — | Safe area, 16:9. |
| T-017 | 2 | Pause, settings, game over, confirm popups | todo | — | Saves to PlayerPrefs. |
| T-018 | 2 | Chronicle/profile screen with stats and avatar | todo | — | Local Wanderer profile. |
| T-019 | 2 | Keyboard/gamepad focus and navigation | todo | — | Visible focus ring, back/cancel. |
| T-020 | 2 | Screen transitions and fades | todo | — | 0.42 s veil. |
| T-021 | 3 | Parallax shrine background and fog layers | todo | — | Slow drift, vignette. |
| T-022 | 3 | Divine altar and god medallion art (procedural) | todo | — | Palette-compliant. |
| T-023 | 3 | AudioMixer, music transitions, SFX bank | todo | — | CC0/OFL only. |
| T-024 | 3 | UI/card/impact sounds, pitch variation | todo | — | No clipping. |
| T-025 | 4 | Divine Favour and patron blessings | todo | — | Flame/Tide/Stone/Gale. |
| T-026 | 4 | Omens per round | todo | — | Six omens, banner + tooltip. |
| T-027 | 4 | Offerings at the shrine | todo | — | Sacrifice hand card → favour + draw. |
| T-028 | 4 | Patron selection in deck builder | todo | — | Three gods. |
| T-029 | 4 | Balance pass and feature teach screens | todo | — | Numbers in data assets. |
| T-030 | 5 | Folder restructure, asmdefs, namespaces | todo | — | Move with .meta; compile after. |
| T-031 | 5 | Remove singletons, FindObjectOfType, per-frame rebuilds | todo | — | Composition root. |
| T-032 | 5 | Object pooling and zero per-frame allocations | todo | — | Cards, numbers, particles. |
| T-033 | 5 | EditMode tests for rules/economy/features | todo | — | NUnit. |
| T-034 | 5 | PlayMode smoke test for a full battle turn | todo | — | UnityTest. |
| T-035 | 5 | Profiling before/after and PERFORMANCE.md | todo | — | Profiler + frame timings. |
| T-036 | 5 | .editorconfig, coverage of .gitignore and LFS | todo | — | No secrets. |
| T-037 | 6 | Rewrite README with media and docs links | todo | — | Honest feature list. |
| T-038 | 6 | Final QA, phase tags, merges and push | todo | — | All phases. |
