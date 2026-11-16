# Testing

## Automated
EditMode (pure rules, fast):
```
Unity.exe -batchmode -nographics -quit -projectPath <path> \
  -runTests -testPlatform EditMode -testResults TestResults/editmode.xml
```
PlayMode (scene + UI smoke):
```
Unity.exe -batchmode -nographics -quit -projectPath <path> \
  -runTests -testPlatform PlayMode -testResults TestResults/playmode.xml
```
A test run passes only when every test passes and the log contains no `error CS`,
`NullReferenceException` or `Exception` caused by the game.

## Manual QA checklist
### Boot
- [ ] `Shrine` scene loads without console errors.
- [ ] 1920×1080, 1600×900, 1280×720 and 16:10 window all remain readable.
- [ ] Title background animates; no tween warnings.

### Flow
- [ ] New profile → Covenant → 8 cards → Seal → battle.
- [ ] Returning profile keeps name, deck, patron, stats and settings.
- [ ] Every screen opens/closes with Esc/back and returns focus.
- [ ] Settings persist across restart and change volume immediately.

### Battle
- [ ] Player always starts; turn banner matches state.
- [ ] Mana starts at 1, grows to 10, refills each round.
- [ ] Card cannot be played without mana; message is clear.
- [ ] Field card attacks once per turn; exhausted cards dim.
- [ ] Combat math matches the rules; elemental effects trigger once.
- [ ] Favour fills from favoured cards/offerings; blessing spends it once.
- [ ] Omen announced every round and its rule is visible in the tooltip.
- [ ] Offering sacrifices a card, draws one and grants favour.
- [ ] Victory/defeat only triggers once; buttons work; no lingering hit stop.
- [ ] Pause during animations cannot corrupt the turn.

### Quality
- [ ] No console errors or new warnings in a full session.
- [ ] Frame time stays under 8 ms at 1080p on the development PC.
- [ ] No per-frame GC allocation from UI updates (Profiler GC Alloc column).
- [ ] No missing sprites, fonts or audio.
- [ ] Keyboard-only and gamepad-only playthrough of one full battle.

## Results
| Phase | Date | EditMode | PlayMode | Manual | Notes |
|---|---|---|---|---|---|
| 0 | 2026-11-16 | n/a | n/a | original loads | Baseline; UI broken in places. |
