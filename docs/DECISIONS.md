# Decisions

Short log. Newest last. Format: date — decision — options — reason.

## 2026-11-16 — Use installed Unity 6000.3.22f1, upgrade the project
- Options: install 2022.3.44f1 (matches original), or upgrade with the installed
  Unity 6.3 LTS.
- Decision: upgrade with 6000.3.22f1 at the user's explicit direction ("use the
  version we have"). The upgrade is committed separately; `master` still has the
  original 2022 project if a rollback is ever needed.
- Consequences: TMP examples broke and were removed; `com.unity.ugui` and
  `com.unity.test-framework` added to the manifest. All future APIs target 6000.3.

## 2026-11-16 — Keep the built-in render pipeline
- Options: move to URP for post-processing and shaders, or stay built-in.
- Decision: stay built-in. The game is 100 % uGUI; a pipeline switch would add risk
  and change the painted art's colour response. Post-processing is achieved with
  screen-space overlays and additive UI layers, matching the Style Bible.
- Consequences: no URP volume stack; custom effects are UI shaders/sprites.

## 2026-11-16 — Replace the fake login/account wall with a local Wanderer profile
- Options: keep plaintext username/password auth, add a real backend, or use a local
  profile.
- Decision: local profile. The old system stores plaintext passwords in PlayerPrefs,
  provides no real security, and blocks the core loop. The profile keeps name, avatar,
  deck, stats and pilgrim rank locally.
- Consequences: `UserManager`/login scene are retired from the build; `ProfileScreen`
  is reimagined as the Chronicle. Existing PlayerPrefs users can still be migrated by
  a one-time migration in `ProfileStore`.

## 2026-11-16 — Use a single runtime-built scene for all screens
- Options: repair six fragile scenes by hand, or build one scene with a composition
  root and generate screens at runtime.
- Decision: one `Shrine` scene with a bootstrap and a runtime UI layer. This makes
  layout, transitions, focus and tests deterministic and avoids serialized wiring
  that broke during the Unity 6 upgrade.
- Consequences: the old scenes stay in `Assets/Scenes` as reference but are removed
  from build settings. UI is code-reviewed and versioned, not clicked together.

## 2026-11-16 — Keep DOTween as the tween library
- Options: write a new tween utility, add LeanTween, or use the bundled DOTween.
- Decision: DOTween is already installed, licensed and battle-tested; no new plugin.
- Consequences: all tweens use `DG.Tweening`, killed on disable and with
  `SetLink`/`OnKill`; reduced-motion wraps durations centrally.

## 2026-11-16 — Add divine favour, omens, offerings and patron gods
- Options: add one small feature, or deepen the shrine theme with a connected set.
- Decision: implement the connected set. Favour is the spendable divine resource,
  omens are the round modifier, offerings convert cards into favour, and patrons
  determine the blessing. They share one HUD and one design language.
- Consequences: the battle model needs a pure rules layer, data assets and tests;
  scope is controlled by three patrons and six omens for the first release.

## 2026-11-16 — Card art for the full pool
- Options: leave 14 cards blank, generate new paintings, or use procedural sigils.
- Decision: build a consistent procedural element-sigil treatment for cards without
  painted art, and reuse the three existing paintings. New paintings may be added
  later only if they match the Style Bible's detail level.
- Consequences: no mismatched AI art; every card is visually complete on day one.
