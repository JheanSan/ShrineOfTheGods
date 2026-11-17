# Rival AI guide

The rival is a deterministic planner in `Assets/_Project/Scripts/Gameplay/OpponentAI.cs`.
It reads the battle model, returns an ordered list of actions, and never touches Unity,
time or threads. `BattleScreen.OpponentTurn` executes that list with presentation delays,
so the same plan replays identically in tests and in the build.

Read this file before changing the AI, and change **`AiTuning.cs`, not `OpponentAI.cs`**,
unless you are adding a new behaviour.

## Structure

| Step | Method | What it decides |
|---|---|---|
| 1 | `PlanTurn` | The only public entry point. Clears `LastPlanSummary` and returns the plan. |
| 2 | `ChoosePlays` | The best set of cards the rival can afford this turn. |
| 3 | `PlanAttacks` | Every strike, including cards played this turn. |
| 4 | `Describe` | `plays=N trades=N face=N`, logged as `[AI]` and asserted in tests. |

### `ChoosePlays` - exhaustive affordable-subset scan

The hand is small (8 cards maximum), so the planner scans **every subset** of the hand and
keeps the most valuable one that fits `Mana`, `MaxCardsPerTurn` and the field limit. That is
why the rival buys two efficient cards instead of one expensive body. The scan is capped at
16 candidates so a future balance pass cannot make it exponential in practice.

Card value comes from `ScorePlay`:

```
value = (attack * AttackWeight + defence * DefenceWeight)
      * StatWeight
      + statsPerMana * ValuePerManaWeight
      + random jitter (TieBreakJitter)
```

Attack is amplified while the player is at or below `LowHealth`; defence is amplified while
the rival is behind on board cards. Cheap cards are played first so the mana bookkeeping is
readable in the log.

### `PlanAttacks` - trades, then reach

1. Copy the player's field and track **planned damage per defender**, so two attackers never
   swing at a card the first one already killed.
2. Sort attackers by attack, biggest first, so a kill removes the target for everyone after
   and small cards are never wasted chipping a corpse.
3. For every ready card (field cards plus, when `AttackWithNewCards` is on, cards played this
   turn), score each surviving defender with `ScoreTarget`:
   `damage + kill bonus + threat removed - retaliation loss`.
4. The best target is taken when its score is at or above `MinimumAttackScore`; otherwise the
   attacker holds. **Holding is a real move**: a card that stays on the field blocks the
   player's shrine strikes, which is why Merciless refuses to trade a body away for chip
   damage. An empty field is a free shrine hit for the opponent.
5. When no defender is left **and the player's board is planned empty**, leftover attackers
   hit the shrine. The hero is never attacked while a defender still stands, which is the
   same rule the player plays by.

Combat maths is never duplicated: `EffectiveAttack`, `RetaliationDamage` and `HeroDamage`
live in `BattleModel` and the AI calls them. If you change a combat rule, the planner follows
automatically.

## Difficulty presets

`AiDifficulty` selects a preset in `AiTuning.From`. `ShrineApp` uses `Merciless`.

| Weight | Measured | Aggressive | Merciless |
|---|---|---|---|
| `MaxCardsPerTurn` | 3 | 4 | 5 |
| `AttackWeight` | 1.0 | 1.15 | 1.35 |
| `DefenseWeight` | 1.15 | 0.95 | 0.8 |
| `ValuePerManaWeight` | 1.8 | 2.2 | 2.4 |
| `KillBonus` | 2.8 | 3.5 | 5.0 |
| `ThreatWeight` | 0.45 | 0.6 | 0.9 |
| `ElementEdgeBonus` | 1.5 | 1.5 | 2.2 |
| `LossPenalty` | 0.9 | 0.6 | 1.2 |
| `WastePenalty` | 3.5 | 2.5 | 4.0 |
| `MinimumAttackScore` | 1.5 | 0 | 0.5 |
| `BehindBoardBonus` | 1.8 | 1.4 | 1.1 |
| `LowHealth` | 8 | 12 | 16 |
| `LowHealthAggression` | 1.25 | 1.6 | 1.9 |
| `AttackWithNewCards` | no | yes | yes |

`Measured` holds cards back and only takes clean trades. `Merciless` spends nearly every
point of mana, contests the element cycle with `ElementEdgeBonus` and puts fresh cards
straight into combat **when the trade pays**. It is the most aggressive preset, not the
most reckless one: `LossPenalty` and `WastePenalty` are highest there because a thrown-away
body is how the rival loses the shrine.

## Second-player compensation

The pilgrim always acts first. To stop a free turn-one strike at a naked shrine, the rival
deploys its strongest opening card as a **guardian** before the first turn
(`BalanceConfig.StartingGuardians`, default 1). The guardian is not a special card; it is a
normal body taken from the rival's hand and placed on the field, and it may attack on the
rival's first turn. Set the count to 0 in tests that need an empty rival field.

## Invariants (tests pin these)

- The rival only acts on its own turn (`PlanTurn` returns an empty list otherwise).
- No plan attacks the shrine while a defender survives it.
- Leftover attackers do hit the shrine once the board is planned clear.
- Mana is never overspent.
- `LastPlanSummary` matches the actions (`plays`, `trades`, `face`).
- The element cycle is `Fire > Air > Earth > Water > Fire`, doubled both ways
  (attacker damage and retaliation). `ElementRulesTests` guards it.

## The element cycle, for players and AI alike

`ElementRules` is the single source of truth. `BattleModel`, the AI, the card ribbon, the
legend and the tooltip all read from it.

| Attacker | Doubles against |
|---|---|
| Fire | Air |
| Air | Earth |
| Earth | Water |
| Water | Fire |

A card that strikes its beaten element deals **double**; a defender that beats its attacker
deals **double** retaliation. The combat forecast in `BattleScreen` shows damage, `KILL`,
reply damage and `CARD DIES` before the card leaves the hand.

Card powers stay separate from the cycle:

- Fire gains the omen's attack bonus.
- Water gains 2 defence the first time it strikes.
- Earth ignores the first damage it would take (the first retaliation) and gains 2 defence.
- Air's first omened gale costs 1 less.

## Verifying a change

```
& "C:\Program Files\Unity\Hub\Editor\6000.3.22f1\Editor\Unity.exe" `
  -batchmode -nographics -projectPath <repo> -runTests -testPlatform EditMode `
  -testResults results-editmode.xml
```

Then build and run the harness:

```
Builds\ShrineOfTheGods.exe -autobattle -shotdir C:\temp\ai-run
```

The recorder plays a strong greedy player and writes `strike_NN` frames (taken 0.16 s after
each action, catching trails and damage numerals) plus the usual action and turn frames.
`Player.log` gets `[AI] round=... plays=... trades=... face=...` lines, so a plan can be
replayed and compared. A rival win, or a player finished under 5 life, is the bar for
"Merciless is actually hard".

### Reading the log

```
[AI] round=7 side=Opponent mana=8/8 hand=5 field=0 actions=4 plays=2 trades=0 face=2
[AI] done field=2 mana=0
```

`plays` is the number of cards bought, `trades` the card attacks, `face` the shrine strikes.
`done` reports the board and mana left **after execution**, so a mismatch with the plan means
an action was rejected by the model (usually an illegal target).
