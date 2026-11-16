# Changelog

All notable changes to *Shrine of the Gods*, newest first.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

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
