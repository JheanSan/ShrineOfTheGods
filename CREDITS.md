# Credits

## Original project
- **Shrine of the Gods** — concept, original code, painted backgrounds, card frame,
  card back, god portraits and card paintings by the project author (JheanSan).
  The repository does not include a separate art license; treat the original art as
  all rights reserved by its author.
- Original music file `Vinnie Camilleri - Viking War Drums.mp3` is **not used** by the
  overhaul. Its license is not documented in the repository; it remains in history
  only and must be cleared or replaced before any public distribution.

## Engine and packages
| Asset | Author | License | Use |
|---|---|---|---|
| Unity 6000.3.22f1 | Unity Technologies | Unity Personal / Unity terms | Engine |
| Unity UI (uGUI) 2.0.0 | Unity Technologies | Unity Companion License | All interface |
| TextMesh Pro (in uGUI) | Unity Technologies | Unity Companion License | All text |
| Unity Test Framework 1.6.0 | Unity Technologies | Unity Companion License | Tests |
| DOTween (free) | Daniele Giardini / Demigiant | DOTween free license (commercial use allowed) | Tweening |

## Fonts
| Font | Author | License | Use |
|---|---|---|---|
| Cinzel | Natanael Gama | SIL Open Font License 1.1 | Display headings |
| Cormorant Garamond | Christian Thalmann | SIL Open Font License 1.1 | Body text |

License texts are stored next to the fonts in `Assets/_Project/Art/Fonts/Licenses/`.

## Procedural art
`ProceduralArt` generates the button frames, card frames, element sigils, altar,
favour motes and vignette at runtime from the Style Bible palette (original code,
no outside images). The original painted backgrounds, card paintings, card back
and god portraits from the project author are reused unchanged.

## Audio
All music and the ambient bed are **procedurally synthesised at runtime** by
`ProceduralAudio` (original code, no outside samples). The impact and interface
samples below are layered under that synthesis for weight and grit.

| Sample set | Author | License | Use |
|---|---|---|---|
| RPG Audio | Kenney (kenney.nl) | CC0 1.0 | Weapon swings, card draw/play, offering |
| Impact Sounds | Kenney (kenney.nl) | CC0 1.0 | Metal hits, plate blocks, body wounds, deaths, bell tolls, mana shatter |
| UI Audio | Kenney (kenney.nl) | CC0 1.0 | Hover, click, toggle |
| Interface Sounds | Kenney (kenney.nl) | CC0 1.0 | Back, deny, confirm |

Curated clips are stored in `Assets/Resources/Shrine/Audio/`. Each cues mixes a
recorded sample with a synthesised sub layer; see `AudioService.cs`.

## Tools
- Documentation and code authored with OpenCode.
- Screenshots captured from the running Windows build.
