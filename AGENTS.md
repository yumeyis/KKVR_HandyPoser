# AGENTS.md: guide for AI coding agents

This file is the entry point for any AI agent (Claude Code, Codex, Copilot, Cursor, ...) working on this repo.
Read it fully, then read [docs/STATUS.md](docs/STATUS.md) before changing anything.

## What this project is

**KKVR_HandyPoser** is a BepInEx 5 plugin for **Koikatsu (コイカツ！/ Koikatsu Party) official VR**
(`KoikatuVR.exe` / `Koikatsu Party VR.exe`). Players grab characters' body parts with VR controllers
and pose them: hold to drag, release to pin, double-click to reset.
The game is assumed to have **HF Patch** installed (BepInEx 5.4.x + HarmonyX).

- User-facing docs: [README.md](README.md) (EN), [README.zh-TW.md](README.zh-TW.md)
- Design: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)
- Build setup: [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md)
- **Current progress, verified/unverified items, next tasks: [docs/STATUS.md](docs/STATUS.md)**

The maintainer (GitHub `yumeyis`) communicates in **Traditional Chinese**. Reply in Traditional Chinese.
Code, comments, commit messages and English docs stay in English.

## Hard rules

1. **Never commit game files.** `Assembly-CSharp*.dll`, `UnityEngine*.dll`, etc. are proprietary.
   `lib/*.dll` and `Directory.Build.user.props` are git-ignored. Keep it that way.
2. **Do not copy code from `reference/KK_SetParentVR`.** It has no license (and is itself decompiled).
   Use it only to learn which game APIs and hook points exist. `reference/` is git-ignored. To get it:
   `git clone https://github.com/MayouKurayami/KK_SetParentVR reference/KK_SetParentVR`
3. **Target is .NET 3.5 (Unity 5.6 Mono).** `net35` + C# `latest` syntax. Not available:
   `Enum.HasFlag`, `ValueTuple` / tuple syntax, `async`/`Task`, `string.Join(string, IEnumerable<string>)`,
   `HashSet` is fine (System.Core). Use the `BodyPartGroup.Contains()` extension instead of `HasFlag`.
4. **Unity null checks:** never use `?.` / `??` on `UnityEngine.Object` (destroyed objects are not C# null).
   Use explicit `!= null`.
5. **Don't claim it works unless it was verified.** Most game-API usage is unverified (see STATUS.md).
   If you cannot build or run the game in your environment, say so and mark items as unverified.
6. Keep `Core/` VR-runtime-agnostic. Runtime-specific code belongs in `Controls/` and `Patches/`.

## Build

The VR build's `Assembly-CSharp.dll` is **not on NuGet** (IllusionLibs only ships the non-VR one, which
lacks `VRHScene` / `VRViveController` / SteamVR). The build needs a local game install:

```bash
cp Directory.Build.user.props.example Directory.Build.user.props   # set KKDir
#   or: tools/copy-game-libs.sh "<game dir>"   /   tools/copy-game-libs.ps1 -GameDir "<game dir>"
dotnet build                # Debug, auto-deploys to <KKDir>/BepInEx/plugins/KKVR_HandyPoser/
dotnet build -c Release     # also produces dist/KKVR_HandyPoser_v<ver>.zip
dotnet format whitespace --folder --verify-no-changes   # what CI runs; works without game DLLs
```

If the game DLLs are missing, the build stops early with an explanatory error (`CheckGameAssemblies`
target in `Directory.Build.targets`). That is expected on machines without the game. Do not
"fix" it by committing DLLs or adding stubs.

There are no automated tests. Verification means building and testing in the VR H scene with a headset.
Logs: `<game>/BepInEx/LogOutput.log`. Debug-level messages need `LogLevels = All` in `BepInEx.cfg`.

## Code map

| Path | Responsibility |
|---|---|
| `src/KKVR_HandyPoser/HandyPoserPlugin.cs` | `[BepInPlugin]`, config entries (`Config.Bind`), Harmony setup. **Version constant lives here too** |
| `Core/HandyPoserController.cs` | Per-H-scene `MonoBehaviour`: per-controller state machine (hover → grab → pin → reset), target rescan, IK re-apply in `LateUpdate` |
| `Core/PoseTarget.cs` | One `ChaControl` + its `FullBodyBipedIK` + list of handles |
| `Core/BodyPartHandle.cs` | `EffectorHandle` (FBBIK effector) and `BendGoalHandle` (elbow/knee bend goal). Anchor GameObject lifecycle |
| `Core/Enums.cs` | `BodyPart`, `BodyPartGroup` (flags), `PinSpace` |
| `Controls/IControllerInput.cs` | Runtime-agnostic input interface, `ControllerSide`, `VRButton` |
| `Controls/OfficialVRControllerInput.cs` | Reads SteamVR device from `VRViveController.device` (private field) |
| `Patches/HSceneHooks.cs` | `VRHScene.MapSameObjectDisable` (init), `VRHScene.SetLocalPosition` (reset) |
| `Patches/ControllerInputHooks.cs` | Blocks the grab button from the game while posing |
| `Visual/MarkerRenderer.cs` | Sphere markers per handle |

The namespace for input is `KKVR_HandyPoser.Controls` on purpose. A namespace named `Input` would
shadow `UnityEngine.Input` inside `KKVR_HandyPoser.*`.

## Conventions

- Style: `.editorconfig`. 4 spaces, LF, braces on new lines, `_camelCase` private fields.
- Config keys are user-visible strings (`"Grab radius"`). **Renaming one resets users' settings.** Avoid it.
- Wrap Harmony hook bodies in try/catch and log via `HandyPoserPlugin.Log`. A throwing hook can break the game scene.
- Version bumps: change **both** `<Version>` in `Directory.Build.props` and `HandyPoserPlugin.Version`.
- Every user-visible change: add a line under `## [Unreleased]` in `CHANGELOG.md`.
- New config entries: also document them in both READMEs.
- Branch from `main` (`feature/...`, `fix/...`). Commit messages in imperative English.

## Before you finish a session

Update [docs/STATUS.md](docs/STATUS.md): what you changed, what was verified (and how), what is still
open. That file is the handoff to the next agent.
