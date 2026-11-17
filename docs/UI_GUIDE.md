# UI Guide

Reference resolution 1920×1080, Canvas Scaler = Scale With Screen Size, match 0.5.
Safe area: 64 px left/right, 48 px top/bottom. Minimum supported 1280×720.

## Layout regions (battle)
```
┌──────────────────────────────────────────────────────────┐
│ patron / omen banner                      settings  ⏸    │ 88
│ opponent avatar + name            HP bar                 │
│             opponent field (5 slots)                     │
│                                                          │
│                  THE SHRINE (altar)                      │
│             player field (5 slots)                       │
│ player avatar + HP     mana pips     divine favour       │
│            hand fan (8 cards, hover lift)                │
└──────────────────────────────────────────────────────────┘
```
The shrine is the visual anchor: a circular glowing altar at screen centre, slightly
below middle. It reacts to offerings, blessings and low HP.

## Components

### Gilded panel
- 9-sliced card frame or a procedural border: 6 px bronze outer, 2 px ember gold
  inner hairline, 12 px transparent padding.
- Fill: `#14182B` at 88 %; optional blur not required.
- Radius: use the painted frame's organic corners, never a CSS pill.

### Button
States: normal, hover, pressed, focused, disabled, selected.
- Normal: bone text, gold hairline at 35 %.
- Hover: gold hairline 100 %, text candlelight, scale 1.035.
- Pressed: scale 0.965, fill darkens 12 %.
- Focused: 2 px cyan-gold ring outside the frame + small caret.
- Disabled: moon silver 45 %, no hover.
- Click sound at −12 dB, hover at −18 dB.
- Minimum hit area 44×44 px.

### Card
- 220×320 px at 1080p (hand), 180×260 on field.
- Frame: existing `Fondo de cartas seleccion.jpeg` sliced.
- Art window 188×150, dark inner well behind it; procedural element sigil when no
  painting. Name plate at top, cost gem top-left, ATK/DEF plaques bottom corners.
- Hover: lift 18 px, scale 1.06, 2° tilt toward cursor, glow.
- Drag: 3° tilt, shadow, valid target highlights gold, invalid target dims.
- Ready-to-attack: thin cyan pulse along the frame.

### Stat bar
- Height 18 px, inner shadow, gold cap at the end, number in Cinzel.
- Damage preview: blood ember ghost segment that drains after 0.35 s.
- Mana: 10 diamond pips, filled cyan, spent dark, glow on refill.
- Favour: 12 thin rays around a sunburst; 0–100 fills rays in order, full = pulse.

### Tooltip
- 320 px max width, appears after 0.35 s hover, offset 16 px from cursor, clamped
  to safe area. Title Cinzel 22, body Cormorant 18, keyword line gold.
- Never covers the hovered card; flip side if needed.

### Modal
- Scrim `#0B1020` 78 %, click outside does not dismiss destructive modals.
- Panel 640–860 px, title 40, content, two buttons max. Esc = cancel, Enter = confirm.
- Focus trap; returns focus to the invoking control.

## Screens
1. **Title** — painted castle/dragon background, animated fog and embers, title
   "SHRINE OF THE GODS", tagline, buttons: Enter the Shrine, Covenant, Chronicle,
   Rites, Leave. Profile badge bottom-left, version bottom-right.
2. **Covenant (deck)** — 8 slots at top, 17-card collection grid below, element
   filters (All/Fire/Water/Earth/Air), patron choice, "Seal the Covenant" enabled at 8.
3. **Battle** — above layout; pause button, turn banner, omen plaque, offering bowl.
4. **Chronicle** — avatar, name, pilgrim rank, win rate, games, favourite element,
   last deck, reset progress (confirm).
5. **Settings** — Master/Music/SFX sliders, fullscreen, resolution (windowed),
   screen shake, reduced motion, UI scale, reset defaults. Save on change.
6. **Pause** — Resume, Settings, Concede (confirm), Title.
7. **Game over** — Victory/Defeat, XP, stats delta, Offerings earned, Play Again,
   Return to Shrine.

## Navigation
- Mouse, keyboard and gamepad parity.
- Tab/Shift+Tab and d-pad move focus spatially; Enter/A activates; Esc/B backs.
- No focus traps except modals. A visible 2 px gold-cyan focus ring is mandatory.
- Scroll views: mouse wheel, right stick, page up/down.
- Screen transitions: fade veil 0.42 s, content enters 24 px upward, staggered 40 ms.

## Copy rules
- Second person, ceremonial but clear: "The shrine accepts your offering."
- Never blame the player. Errors state the fix: "Your covenant needs 8 cards."
- Numbers always show units/icons; no unexplained abbreviations.

## Card face and board (Phase 7)

Card: **172 x 230** canvas px, hand scale 0.92, field scale 0.98. Hand spacing
186 px, so a five-card hand never overlaps; the hand root sits at y -390 so the
lowest card clears the screen edge.

Card anatomy, top to bottom:
- cost gem (0.22 w) top-left, overhanging the frame; the number is 0.115 w.
- name plate: 0.92 w band, name at 0.086 w with 3 pt tracking.
- element line under the plate, 0.058 w, 55 % desaturated element colour.
- art well: 0.88 w x 0.44 h, recessed, with a vignette and a per-card painted scene.
- rule at -0.163 h, then `ATK` and `LIFE` labels at 10 px, then the two gems at
  -0.33 h. Labels sit above the gems, never on them.
- frames are 9-sliced; the outer 7 px is carved shadow, 22 px brackets are gilded.

Board:
- two 1020 x 240 field panels, five 172 x 230 slots each at 190 px pitch.
- idle slots draw corner brackets at 55 % white; while dragging, the slot the card
  would fill goes to 100 % candlelight and the rest dim to 28 %.
- hero plates sit in the side margins: portrait, name, patron, `LIFE n/n` under
  the bar, then the socketed life total (72 px) and a shield badge when shielded.
- the mana bar shows all ten sockets from round one: bronze outline when locked,
  cyan fill when available, dim when spent.
- the turn plaque shows cards remaining above `CARDS REMAIN`; `VICTORY`/`DEFEAT`
  replaces the count when the battle ends. The blessing button reads
  `BLESSING n%` until it is charged, then `CLAIM BLESSING`.