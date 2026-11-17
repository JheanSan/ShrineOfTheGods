# Style Bible — Shrine of the Gods

This is the visual contract. Any new asset, shader, particle, screen or animation must
pass this page. The original look is **painted dark fantasy at the edge of twilight**:
a sacred ruin under a violet sky, lit by ember gold and cold spirit-light. It is
romantic and ceremonial, never cartoonish, never neon-cyberpunk, never generic mobile UI.

## 1. Palette

| Role | Hex | Use |
|---|---|---|
| Midnight ink | `#0B1020` | deepest background, letterboxing, modal scrim |
| Shrine shadow | `#14182B` | panels, card wells, unlit stone |
| Twilight indigo | `#222A48` | elevated panels, HUD bars |
| Forest violet | `#2E1B3E` | atmospheric wash behind battle |
| Amethyst haze | `#6E4A8C` | magic, card backs, omen wisps |
| Spirit cyan | `#8FE3E8` | divine sparks, air, focus glow |
| Ember gold | `#D9B36C` | primary accent, borders, headings, hover |
| Deep bronze | `#8A6538` | frame undersides, dividers, pressed states |
| Candlelight | `#F4D79A` | text highlights, embers, victory rays |
| Bone parchment | `#EFE6D3` | body copy on dark surfaces |
| Blood ember | `#C4523C` | damage, fire, defeat, danger |
| Moss devotion | `#719B7A` | healing, nature, success |
| Moon silver | `#B9C2D6` | muted secondary text, disabled |
| Ember orange (fire) | `#E0613F` | fire cards, fire damage, ember scenes |
| Deep tide (water) | `#3E9FC4` | water cards, tide damage, water scenes |
| Ochre stone (earth) | `#C8A34A` | earth cards, earth damage, stone scenes |
| Pale gale (air) | `#BFD4E8` | air cards, gale damage, sky scenes |

Rules: gold is a **highlight**, never a fill. Cyan and violet are light sources, not
flat backgrounds. Damage always reads blood ember; healing reads moss; divine power
reads cyan-to-gold. Never introduce saturated magenta, pure white (`#FFFFFF`) or pure
black; the darkest value is midnight ink.

### Element identity (non-negotiable)
The four element hues above are **interface colours**, deliberately more saturated than
the stone palette, because a 172 px card must announce its element before anything else:

1. **Ribbon.** A full-width band in the element hue under the art, carrying the element
   glyph and name on the left and the element it defeats on the right (`EARTH > WATER`).
2. **Art scene.** Every card paints its own scene in the element's key: fire = molten
   river and ember sky, water = moonlit waves and rain, earth = stone pillars and dust,
   air = floating islands and wind streaks. The creature silhouette is seeded from the id.
3. **Glyph watermark.** The element sigil repeats faintly over the art and again in the
   legend, so the cycle is learnable without reading the tooltip.
4. **Combat colour.** Damage numerals, trails, impacts and the `ELEMENTAL EDGE` callout
   are tinted with the attacker's element. Retaliation is tinted with the defender's.

## 2. Shape language
- **Arches and pointed ovals**: shrine windows, god medallions, card tops.
- **Ornate gilded frames** with a thin inner hairline and a heavier outer rail.
  Corners carry a small gem or scroll motif. Borrow the existing card frame and
  `MainMenuBoton.png`; do not invent square Material buttons.
- **Ritual geometry**: circles, concentric rings, radial spokes, mandalas.
  The card back is the reference for sacred detail density.
- **Hand-painted edges**: silhouettes are slightly irregular. Do not use perfect
  rounded-rectangle mobile cards without a frame.
- Line weight: 2 px inner hairline at 1080p, 6–10 px outer ornament. Avoid uniform
  1 px outlines on large shapes.

## 3. Lighting and atmosphere
- Key light: low, warm ember from below/side (candles, brazier, shrine fire).
- Fill: cold cyan/violet moonlight from above and behind.
- Every screen has depth: far painted background, mid atmospheric fog, near UI frame.
- Add drifting motes/embers, never confetti. Motion is slow, 6–20 px/s.
- Vignette is always present but subtle (12–25 %); center stays readable.
- Glows bloom outward in 2–3 soft rings. No hard neon outlines.

## 4. Surfaces and materials
- Dark polished wood/stone: low roughness, soft specular streak, not glassy.
- Gilded metal: two-tone — warm highlight and dark bronze shadow.
- Parchment/leather: visible grain, slightly warm, never pure grey.
- Card art is painterly and desaturated except for its element accent.

## 5. Typography
- Display: **Cinzel** (OFL), wide-tracked small caps, ember gold or candlelight.
- Body: **Cormorant Garamond** (OFL), 18–26 px at 1080p; never below 16 px.
- Numerals on stats may use Cinzel for ceremony, body font for dense rules text.
- Letter-spacing: display +8–14 %, body +1 %. No all-caps body text.
- Text is never pure white; use bone parchment. Disabled text is moon silver at 45 %.

## 6. Iconography
- Elements: Fire = three-tongued flame; Water = crescent wave; Earth = mountain rune;
  Air = spiral gust. Draw as engraved gold line icons, not emoji.
- Divine favour: a sunburst with an eye at the centre.
- Curse/omen: an inverted crescent with three thorns.
- Offerings: a shallow bowl with rising sparks.

## 7. Motion personality
- Ceremonial, weighty, deliberate. Nothing snaps like a mobile game.
- Anticipation before power, strong impact frame, slow settle. See
  `ANIMATION_GUIDE.md` for exact timings.
- Idle motion is breathing (scale 1.00–1.015, 3–5 s) and drifting light.
- UI transitions feel like curtains/veils, not slides from a spreadsheet.

## 8. Composition
- 16:9, 1920×1080 reference. Safe area 64 px sides, 48 px top/bottom.
- Keep the shrine visible: never let the HUD cover more than 22 % of the screen height.
- Focal hierarchy: shrine/god art → cards in play → hand → HUD → ornament.
- Empty space is part of the ritual. Do not fill every corner.

## 9. Do / Don't

**Do**
- Reuse the painted backgrounds, card frame, card back and gilded button art.
- Layer gold hairlines over deep translucent indigo.
- Let particles be sparse and meaningful (embers, motes, divine glyphs).
- Animate with easing and overshoot; let impact breathe.
- Keep card text legible over art with a dark inner well and 80 % scrim.

**Don't**
- Use flat bright blue, purple neon, rounded mobile chips or Material Design.
- Add sci-fi holograms, glitch effects, emoji, comic sans or modern sans display type.
- Cover the painted background with opaque panels.
- Use rainbow gradients, drop shadows with 0 blur, or pure black outlines.
- Add new art with a different rendering level/detail than the existing paintings.
- Use animation for decoration without gameplay meaning.

## 10. Acceptance checklist
- [ ] Colours come from the palette (or a documented tint of one).
- [ ] Gold behaves as an accent, not a surface.
- [ ] Type uses Cinzel/Cormorant with correct sizes and tracking.
- [ ] The painted background is still visible and atmospheric.
- [ ] Motion has anticipation, impact, follow-through; no linear tweens.
- [ ] Screenshot at 1920×1080 reads clearly at 50 % scale.
- [ ] Nothing looks like a different game.
