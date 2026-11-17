# Changelog

All notable changes to *Shrine of the Gods*, newest first.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Phase 8 — Combat overhaul, element identity and a planning rival
#### Added
- `ElementRules`: the elemental cycle in one place (`Fire > Air > Earth > Water >
  Fire`). Attacker damage and retaliation both double across an advantage, and the
  AI, card ribbon, legend and tooltip all read the same table.
- Element-coloured card faces: a full-width ribbon with the element glyph and the
  element it defeats (`EARTH > WATER`), a sigil watermark over the art and an
  element-tinted frame.
- Per-element painted card scenes in `ProceduralCardScenes`: molten river and ember
  sky, moonlit waves and rain, stone pillars and dust, floating islands and wind
  streaks, each with a seeded creature silhouette.
- Combat forecast panel: while a field card is dragged it shows damage, `KILL`,
  reply damage and whether the card dies, plus the `DOUBLE DAMAGE` line when the
  cycle applies.
- Persistent `ELEMENT CYCLE` legend under the rival plate.
- Element-tinted combat feedback: trails between attacker and target, tinted damage
  numerals, `ELEMENTAL EDGE` callout and a tinted impact ring.
- `AiTuning` with `Measured` / `Aggressive` / `Merciless` presets, an exhaustive
  affordable-hand search, planned-damage tracking per defender, attacks with cards
  played the same turn, and leftovers sent at the shrine after the board is cleared
  (see `docs/AI_GUIDE.md`).
- `ElementRulesTests` and a leftover-attacker AI test.

#### Changed
- Card face layout rebuilt around the element band: name plate on top, art well,
  element ribbon, labelled ATK/LIFE gems.
- Deck curve flattened: the 6–8 cost cards were traps that left both sides with
  unplayable hands (a 5/4 Chimera cost 8, an equal Gorgon 8, while a 5/3 Phoenix
  cost 2). Costs now run 2–6, so every opening hand has a play and the rival can
  actually spend its mana. Regenerated card assets with the scene builder.
- The rival now values its board: it holds an attack that kills nothing and costs
  it a body, sorts attackers biggest-first, prefers the element that beats the
  player's board (`ElementEdgeBonus`) and only accepts a death for a kill. This is
  what turned the old "trade everything, empty field, lose" behaviour around.
- The rival opens with its cheapest card already on the field (a guardian) as
  second-player compensation, taken before the playable-hand guarantee so its
  opening hand stays affordable.
- `BattleScreen` combat now colours every effect by the striking element and the
  reply by the defender's element.
- `BattleRecorder` also captures a `strike_NN` frame 0.16 s after each action so
  trails, callouts and numerals can be verified in stills.

#### Fixed
- Card ribbon labels were clipped: the element tag, `ATK` and `LIFE` labels used
  text rects barely taller than the glyphs. Small labels now overflow instead of
  truncating, and the tag sits flush inside the ribbon in full ink.
- The playable-opening-hand guarantee could fail when the rival's random draw pile
  held no cheap card; it now falls back to the catalogue.
- Only fire doubled into air; water, earth and air advantages did nothing. All four
  now double, including retaliation.
- Earth's first strike at a shrine dealt **0** damage and consumed its defensive
  power. It now deals full damage; the power stays a defensive reaction.
- `ProceduralArt.CardEmblem` and the old single-scene generator were removed in
  favour of the element scenes; no external art was added.

### Phase 7 — Clarity, presence and full-battle verification
#### Added
- Painted card art: every card gets its own seeded shrine landscape (moon,
  four ridgelines with rim light, ruin gate, foreground colonnade) tinted by
  element, so the art well reads as an illustration instead of a rune.
- Magic/Hearthstone-style board: 172x230 cards, a spread hand that never
  overlaps, two 240 px field rows, and dim idle slots that brighten on drop.
- `HeroPlate`: portrait slate with a 30 HP socket, life bar, shield badge,
  low-life pulse and a rival hand count.
- `ManaBar`: ten crystal sockets, unlocked ones bronze, spent ones dim.
- Recorded CC0 audio from the Kenney packs (25 clips) layered under the
  existing synthesis for weight: 17 cues are now sample-backed.
- `BattleRecorder` `-autobattle` harness: plays a whole battle through the
  normal API, screenshots every action and logs the result.

#### Changed
- Turn plaque now shows cards remaining instead of repeating "YOUR TURN";
  blessing button shows its charge ("BLESSING 42%" / "CLAIM BLESSING").
- Card faces: name plate, element line, ATK/LIFE labels and larger gems.

#### Fixed
- Rival mana never grew past 2: both sides now ramp to 10 and refill on their
  own turn.
- Neither side drew after the opening hand (deck stuck at 20): both draw at the
  start of their turn, and the duplicate rival draw was removed.
- The planner ignored mana and played at most one card: it now spends a
  simulated pool and fills its board.

### Phase 5 — Code quality and tests (in progress)
#### Added
- EditMode rule tests (11) and PlayMode boot tests (2); all passing.
- Card view pooling, a frame probe and a rewritten README.
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
