# Shrine of the Gods — Working Rules

This file is the first thing every session reads. Keep it under 80 lines.

## Mission
Make *Shrine of the Gods* dramatically better in animation, game feel, UI, visuals,
originality and code quality without losing its dark-fantasy shrine identity.

## Hard rules
1. Protect the art style. Read `docs/STYLE_BIBLE.md` before adding anything visual.
2. Never work on `main`/`master`. Branch per phase: `overhaul/phase-N-name`.
   Commit every finished task. Tag finished phases `v-phase-N`. Never force-push.
   Never delete an asset without a written reason in the commit message.
3. Read before writing. Check `ProjectSettings/ProjectVersion.txt`,
   `Packages/manifest.json`, existing code and asset usage first. Never invent APIs.
4. Compile after every change with Unity batchmode (see below). Fix all errors and
   every new warning before moving on.
5. Verify visually. Build/run the game and look at screenshots before claiming it works.
6. No placeholder code, TODOs or fake completion. Outside assets must be CC0 or OFL,
   and logged in `CREDITS.md`.
7. Stay inside the project and repo. Never commit secrets.
8. Decide independently. Ask only for credentials, legal issues or destructive choices.

## Project facts
- Unity: **6000.3.22f1** (project upgraded from 2022.3.44f1; see DECISIONS.md).
- Render pipeline: Built-in. Product: 2D card battler rendered with uGUI.
- Platform target: Windows standalone, 1920x1080 reference, 16:9.
- Packages: uGUI 2.0.0, Test Framework 1.6.0, DOTween (bundled DLL).

## Conventions
- PascalCase types/methods, `_camelCase` private fields, one class per file,
  file name equals class name.
- Constants or ScriptableObjects instead of magic numbers. Events over polling.
- XML summary only on public APIs whose intent is not obvious.
- Conventional Commits, e.g. `feat(ui): animated pause menu`.

## How to compile / test
Unity editor:
`C:\Program Files\Unity\Hub\Editor\6000.3.22f1\Editor\Unity.exe`
```
& "C:\Program Files\Unity\Hub\Editor\6000.3.22f1\Editor\Unity.exe" `
  -batchmode -nographics -quit -projectPath "C:\Users\Admin\Projects\ShrineOfTheGods" `
  -logFile "C:\Users\Admin\AppData\Local\Temp\opencode\unity-compile.log"
```
Tests:
```
... -runTests -testPlatform EditMode -testResults results-editmode.xml
... -runTests -testPlatform PlayMode -testResults results-playmode.xml
```
Build:
```
... -buildWindows64Player "Builds/ShrineOfTheGods.exe"
```
Run the player, then capture with the Windows desktop tools. Never claim a visual
result without inspecting a screenshot.

## Documentation map
`AGENTS.md` rules · `README.md` public face · `CHANGELOG.md` history ·
`CREDITS.md` licenses · `docs/00_START_HERE.md` index · `docs/PROGRESS.md` now ·
`docs/TASKS.md` backlog · `docs/DECISIONS.md` rationale · `docs/BUGS.md` defects ·
`docs/IMPROVEMENT_PLAN.md` roadmap · `docs/STYLE_BIBLE.md` look ·
`docs/ANIMATION_GUIDE.md` motion · `docs/UI_GUIDE.md` interface ·
`docs/ARCHITECTURE.md` systems · `docs/TESTING.md` QA · `docs/PERFORMANCE.md` numbers ·
`docs/media/` evidence · `docs/archive/` old snapshots.

## Session protocol
- Start: read this file, `docs/PROGRESS.md`, `docs/TASKS.md`.
- After each task: update TASKS/PROGRESS/CHANGELOG and commit code + docs together.
- End of phase: update ARCHITECTURE/TESTING/PERFORMANCE, tag, merge into `develop`,
  write a phase report in PROGRESS.md.
