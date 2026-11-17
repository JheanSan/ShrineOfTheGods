# Progress

## Active phase
**Phase 8 — Combat overhaul, element identity and a planning rival** (complete;
tagged `v-phase-8`).

## Last completed task
T-057 — card labels no longer clip and the opening-hand guarantee falls back to
the catalogue when the rival's pile holds no affordable card.

## Next three tasks
1. Phase 9: animated combat staging — attack lunge, hit reaction on the
   defender card, and a slower rival turn so the plan is readable beat by beat.
2. Optional — native AudioMixer asset and sustained-battle profiling.
3. Optional — localisation (English/Spanish).

## Open blockers
- None. Unity 6000.3.22f1, licence and Windows build module work; Windows player builds.
- T-019 (gamepad parity) was dropped: EventSystem navigation is wired but no
  physical controller was available for verification.
- A native `AudioMixer` asset remains a roadmap item; routing uses three
  gain-controlled AudioSources.

## Phase reports

### Phase 8
Goal: make combat make sense, make the elements visible, and make the rival
genuinely hard to beat — then prove it by playing a series of whole matches.

**Combat rules had to be fixed first.** Only Fire beat Air; Water, Earth and Air
advantages did nothing at all, so the cycle printed on the cards was a lie. Worse,
Earth's retaliation consumed its power after a shrine attack, so its first strike
at a hero dealt zero. `ElementRules` is now the single source of truth: Fire > Air
> Earth > Water > Fire, attacker damage doubles and so does the retaliation. The
model, AI, card ribbon, legend and tooltip all read the same table.

**Elements are now visible.** Every card face carries a full-width element ribbon
in the element hue, the element glyph on the left, the element name, and the
element it defeats (`EARTH > WATER`) on the right. Behind the art sits an element
sigil watermark and a painted element scene: molten river and ember sky, moonlit
waves and rain, stone pillars and dust, floating islands and wind streaks. The
battle screen adds a persistent `ELEMENT CYCLE` legend under the rival plate, and
every strike is tinted by the striking element — trail, ring, numerals, and an
`ELEMENTAL EDGE` callout on a doubled hit. A combat forecast shows damage, `KILL`,
reply damage and whether the card dies before the swing is committed.

**The rival learned to hold a board.** Phase 7's planner traded every card it had,
so its field was empty every turn and the recorder walked to the shrine. It now
sorts attackers biggest-first, refuses an attack that kills nothing and costs it a
body, prefers the element that beats the player's board (`ElementEdgeBonus`) and
accepts a death only for a kill. `Merciless` also values its own card more than the
old preset (`LossPenalty` 0.35 -> 1.2, `WastePenalty` 1.5 -> 4.0).

**Turn order was a hidden blowout.** The pilgrim acts first, and a shrine can only
be struck while its board is empty, so the pilgrim could hit a naked shrine on
turn one. The rival now deploys its **cheapest** opening card as a guardian before
the first turn (`BalanceConfig.StartingGuardians`). Cheapest, not strongest: an
early build took the biggest card and dropped a 6-cost Hydra on turn one, which
decided the game before it started.

**Difficulty is measured, not claimed.** Eight autobattles against the recorder
that wins almost everything: **5 rival wins, 3 player wins**, and the player's wins
cost 27-30 life on four of them. Battles run 5-17 rounds and up to 42 actions.

**Defects found by verification.** The 6-8 cost cards were traps that left both
sides with dead hands (a 5/4 Chimera cost 8 while a 5/3 Phoenix cost 2); costs are
now 2-6. The playable-opening-hand guarantee could fail when the rival's random
pile held no cheap card; it now falls back to the catalogue. Small card labels
clipped because their rects were barely taller than the glyphs; they overflow now.
The play flourish was drawn in canvas space while its card was still gliding, so a
ring landed on an empty slot; it is parented to the card. Spent mana crystals were
too bright to read as empty.

### Phase 7
Goal: make the battle read at a glance, move like a modern card battler and
sound heavy, then prove it by playing whole matches.

**Cards.** Every card now paints its own scene: a seeded shrine landscape with a
moon, four ridgelines with lit edges, a ruin gate and a foreground colonnade,
tinted by the card's element. Art sits in a recessed well under a vignette, with
a name plate, an element line and labelled ATK/LIFE gems.

**Board.** Cards are 172x230 with a 0.92 hand scale and a 0.98 field scale; the
hand spreads without overlap, both fields sit in 240 px panels, idle slots are
dim corner brackets and the next slot a dragged card would fill lights gold.

**Readouts.** `HeroPlate` replaces floating text with a portrait slate holding a
30 HP socket, life bar, shield badge, rival hand count and a low-life pulse.
`ManaBar` shows ten sockets so the ramp to 10 is visible from turn one. The turn
plaque shows cards remaining, so "YOUR TURN" is no longer printed twice.

**Audio.** 25 curated CC0 samples from the Kenney packs are layered under the
existing synthesis; 17 cues are sample-backed and the rest keep procedural
weight. Every clip is logged in `CREDITS.md`.

**Gameplay fixes found by verification.** The rival's mana cap never rose past
2, so it could not play its own deck; neither side drew after the opening hand,
leaving both decks at 20 all game; and the planner ignored mana entirely. Both
sides now ramp 2 to 10, draw at the start of their turn, and the planner spends
a simulated pool, filling its board by the late game.

**Harness.** `BattleRecorder` (`-autobattle -shotdir <dir>`) plays a battle
through the public API and screenshots the title, the open, every action, every
turn boundary and the end screen, without touching the user's mouse or keyboard.

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
- EditMode: **11/11 passed**. PlayMode: **2/2 passed**, re-run on the final code.
- Windows player: **build succeeds** (`Build Finished, Result: Success.`).
- Full battles played by `-autobattle` through the normal API, never by injecting
  desktop input. Latest run: victory in round 5, 16 actions, 27 screenshots; the
  longest run reached round 18 with 56 actions and 93 screenshots. A defeat path
  was verified too (`playerHealth=0 rivalHealth=27 round=11`), so both endings render.
- AI telemetry from those runs: both sides ramp 2 → 10 mana, the rival draws every
  turn, and by round 9 it spends all ten mana and holds a full board.
- Audio log: `25 recorded CC0 samples loaded, 17 cues backed by samples.`
- Performance: **16.66 ms/frame at 60 Hz vsync, 0 B/frame** steady state.
