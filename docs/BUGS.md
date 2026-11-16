# Bugs

| ID | Description | Repro | Status | Fix commit |
|---|---|---|---|---|
| B-001 | Unity 6 compile fails in TMP example scripts (`Vector4[]`→`Vector2[]`). | Open project in 6000.3.22f1. | fixed | T-004 (examples removed) |
| B-002 | `DeckManager.Update()` destroys and recreates every card every frame. | Open ManageDeckScene. | open | — |
| B-003 | `OpponentAttackManager` calls `Thread.Sleep(500)` on the main thread. | End player turn. | open | — |
| B-004 | 14/17 cards have no art; `Resources.Load` returns null silently. | Open deck builder. | open | — |
| B-005 | Turn flow uses random first player + `Invoke`, opponent turn can overlap. | Start battle repeatedly. | open | — |
| B-006 | Plaintext passwords stored in PlayerPrefs. | Inspect `UserData` key. | open | — |
| B-007 | Card dragged from hand onto enemy attacks directly, bypassing field play. | Drag any card to portrait. | open | — |
| B-008 | `Draggable` finds CardInteraction/Canvas with `FindObjectOfType` in Awake. | Instantiate card outside battle scene. | open | — |
| B-009 | Game over leaves `Time.timeScale`/interactions in an undefined state. | Die while dragging. | open | — |
| B-010 | `PlayerInfoDisplay` creates a new Texture2D per profile open and never disposes it. | Open profile repeatedly. | open | — |
| B-011 | Original `.gitattributes` marks every word as an LFS filter; clone warns. | `git clone` fresh. | fixed | T-002 |
| B-012 | Original repo tracks `Library/` (2985 files) and `.vs/`. | `git ls-files Library`. | fixed | T-002 |
| B-013 | Battle scene crashes on start with `ArgumentOutOfRangeException` when no deck is built; raw yellow/blue placeholder UI. | Play MainMenu → Play without sealing a deck. | open | — |
