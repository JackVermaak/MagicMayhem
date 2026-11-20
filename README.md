# Magic Mayhem

[![Tests](https://github.com/JackVermaak/MagicMayhem/actions/workflows/tests.yml/badge.svg)](https://github.com/JackVermaak/MagicMayhem/actions/workflows/tests.yml)

A card duel in virtual reality. You stand at a table, pick up spell cards with your hands and play them against a computer opponent. Built in Unity with the XR Interaction Toolkit for Meta Quest.

A team project by four students for the **Innovative Technologies** course at Hochschule Rhein-Waal, summer 2024. It got a 1.3, the highest grade in the class.

## How to play

- You are dealt 5 cards and the computer is dealt 5 from the same 30-card deck. Each card has an element (Fire, Ice or Water) and a strength from 0 to 9.
- Pick up a card and place it on the play spot on the table. The computer answers with a card from its own hand.
- **Fire beats Ice, Ice beats Water, Water beats Fire.** Two cards of the same element: the higher number wins, and equal cards draw.
- A match is three rounds. If the score is level after three, extra rounds are played until someone is ahead or the cards run out. A new match starts a few seconds after the end.

## Open the project

1. Install **Unity 2021.3** (the project was made with 2021.3.20f1) with **Android Build Support** for Quest. Newer 2022.3 versions also open it and upgrade it.
2. Import these free packs from the Unity Asset Store or their authors. They are left out of this repository because their licences do not allow publishing them here, and they make up most of the project's size. Once imported, the scene finds them again.

   | Folder in `Assets` | What it is |
   | --- | --- |
   | `Rocks and Boulders 2` | Rocks around the clearing ([Asset Store](https://assetstore.unity.com/?q=Rocks%20and%20Boulders%202)) |
   | `PBR Table` | The table ([Asset Store](https://assetstore.unity.com/?q=PBR%20Table)) |
   | `GrassFlowers` | Grass and flowers ([Asset Store](https://assetstore.unity.com/?q=Grass%20Flowers)) |
   | `Handpainted_Grass_and_Ground_Textures` | Terrain textures ([Asset Store](https://assetstore.unity.com/?q=Handpainted%20Grass%20and%20Ground%20Textures)) |
   | `LowPolyMedievalPropsLite` | Props ([Asset Store](https://assetstore.unity.com/?q=Low%20Poly%20Medieval%20Props%20Lite)) |
   | `RPGPP_LT` | RPG Poly Pack Lite, trees and scenery ([Asset Store](https://assetstore.unity.com/?q=RPG%20Poly%20Pack%20Lite)) |
   | `ressources/Materials/YughuesFreeMetalMaterials` | Metal materials ([Asset Store](https://assetstore.unity.com/?q=Yughues%20Free%20Metal%20Materials)) |
   | `Unity Technologies/LowpolyCharacterRio` | Character model ([Asset Store](https://assetstore.unity.com/?q=Rio%20Lowpoly%20Character)) |
   | `Plugins/Banana Yellow Games` | Banana Man character ([Asset Store](https://assetstore.unity.com/?q=Banana%20Man)) |
   | `Oculus Hands` | Animated hand models for the controllers, from Meta's Oculus samples |
   | `ressources/Sounds` | Sound effects from Freesound ([580310](https://freesound.org/s/580310/), [669472](https://freesound.org/s/669472/), [685657](https://freesound.org/s/685657/)) and background music from Pixabay |

3. Open `Assets/Scenes/SampleScene.unity`. It is the scene in the build settings. Build for Android with a Quest connected, or press Play with Quest Link.

## Improvements since the course

- **Rules separated from the scene.** The game logic was one large script that also spawned cards, played sounds and updated the text. The rules (elements, the deck, rounds and scoring) are now a separate C# library with no Unity code, and the scene scripts only connect it to what you see.
- **Tests.** 15 NUnit tests cover every element and value pairing, the deck, scoring and how a match ends. They run in Unity's Test Runner and, through `dotnet-tests`, on GitHub Actions without a Unity licence.
- **A fair opponent.** The computer is now dealt its own hand at the start, like the player, and answers from it. Before, it drew a new random card from the deck each round, after yours was played.
- **Ties are handled.** A level score after three rounds ended with the message "ERROR!". Now extra rounds decide the match, and it is a draw only if the cards run out level.
- **Cards play once, where you put them.** Touching the table spot once used to be enough: a card released anywhere afterwards was snapped onto the table and played. A placed card could also be picked up and played again, and a second card could be played while a round was still on the table. Each of these is now prevented.
- **Clearer messages.** Each round says why it was won or lost ("Water beats Fire. You win the round!"), the instructions explain the rules, and a new message no longer gets hidden early by an older one's timer.
- **Tidier project.** Unused scripts and the unused UModeler package are removed, the game scene is in the build settings, missing references are reported instead of crashing, and the inspector shows tooltips for every setting.

## Files

```
Assets/Scripts/Rules/     the rules: Card, Deck, CardRules, Match (no Unity code)
Assets/Scripts/           scene scripts: CardGameManager, ObjectPlacementHandler (one per card), hand animation
Assets/Tests/EditMode/    NUnit tests for the rules
Assets/Game Cards/        card art and materials
dotnet-tests/             runs the same tests with .NET, for CI
```

## Tests

In Unity: **Window > General > Test Runner > EditMode > Run All**. Without Unity, with the .NET SDK:

```bash
dotnet test dotnet-tests
```
