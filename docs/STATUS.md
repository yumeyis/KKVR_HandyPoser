# Project status / handoff log

> Keep this file current. It is how one development session (human or AI) hands off to the next.
> Newest entries on top in the session log.

## Current state (2026-11-10)

**Phase:** scaffolding + first implementation done. **Never compiled, never run in game.**

The code was written on a Linux machine without the game or a .NET SDK. Everything that touches game
types is based on reading `reference/KK_SetParentVR` and general knowledge of Koikatsu / FinalIK / SteamVR 1.x.
Expect compile errors on the first real build.

## Verification checklist

Tick items only after they were actually verified, and note how.

### Build
- [ ] `dotnet restore` succeeds (IllusionMods feed reachable, `IllusionLibs.BepInEx` 5.4.22, `IllusionLibs.BepInEx.Harmony` 2.9.0)
- [ ] `dotnet build` compiles against the VR `Assembly-CSharp.dll`
- [ ] Release build produces `dist/KKVR_HandyPoser_v0.1.0.zip` with `BepInEx/plugins/KKVR_HandyPoser/KKVR_HandyPoser.dll`
- [ ] `dotnet format whitespace --folder --verify-no-changes` passes

### Unverified game-API assumptions (check with dnSpy or the compiler)
| Assumption | Where |
|---|---|
| `VRHScene.MapSameObjectDisable` exists and runs after characters/IK are ready | `Patches/HSceneHooks.cs:17` |
| `VRHScene.SetLocalPosition` is called on H point change | `Patches/HSceneHooks.cs:35` |
| `VRViveController.IsState/IsPressDown/IsPressUp` are public, single overload, first arg `EViveButtonKind` | `Patches/ControllerInputHooks.cs:19-31` |
| `VRViveController` has private field `device` of type `SteamVR_Controller.Device` | `Controls/OfficialVRControllerInput.cs:21` |
| `SteamVR_Controller.ButtonMask.Grip` matches the physical grip. **Note:** KK_SetParentVR used `1ul << 3` for grip instead of `ButtonMask.Grip` (`1ul << 2`), possibly an Oculus/Index mapping quirk. Test on real hardware | `Controls/OfficialVRControllerInput.cs:125` |
| `VRHScene.sprites` (public `HSprite[]`), `HSprite.flags.nowAnimStateName` | `Core/HandyPoserController.cs:41` |
| `VRHScene.lstMotionIK` (private `List<MotionIK>`), `MotionIK.Calc(string)` | `Core/HandyPoserController.cs:40,279` |
| `ChaControl.visibleAll`, `ChaControl.sex` (0 = male) | `Core/PoseTarget.cs:41,49` |
| VR controller GameObjects are named `Controller (left)` / `Controller (right)` | `Controls/OfficialVRControllerInput.cs` `FindControllers` |

### In-game behavior (official VR, H scene)
- [ ] Plugin loads in the VR exe only (log shows plugin loaded; not loaded in the main game exe)
- [ ] `Attached to VRHScene` and `Tracking <name> (13 parts)` appear in the log
- [ ] Markers appear near the controllers and change color on hover
- [ ] Grip near a hand grabs it, and the hand follows the controller without snapping
- [ ] Releasing pins the hand; it stays while the animation plays
- [ ] Double-click grip resets the part to the animation (also after an animation/motion change)
- [ ] Elbow/knee bend goals change limb bending as expected
- [ ] Hips / shoulders / thighs: check whether this is useful or causes the body to fly off
- [ ] Changing H position resets everything with no errors in the log
- [ ] The game ignores the grip while hovering/grabbing, and grip works normally elsewhere
- [ ] Haptics fire
- [ ] Leaving the H scene: no exceptions, no leftover anchor/marker objects
- [ ] 3P: second female is posable. Male posable when visible and `Allow posing male` is on
- [ ] Coexistence with KK_SetParentVR / KoikatuVRAssistPlugin

## Next tasks (priority order)

1. **First real build on the game machine.** Fix compile errors and tick the API table above.
2. **First in-game test** using the behavior checklist. Record findings in the session log.
3. Fix the IK ordering if parts lag or jitter: apply overrides from `IKSolver.OnPreUpdate` instead of `LateUpdate` (see ARCHITECTURE.md "Known open points").
4. Make the markers visible through the body (always-on-top material/shader).
5. Head/neck posing (not an FBBIK effector: NeckLookController or a look target).
6. Optional: save/restore poses, mirror pose left↔right, per-part "lock rotation" toggle.
7. Optional: support other VR runtimes (KK_MainGameVR / CharaStudio VR) via a new `IControllerInput` + hooks.
8. Once a build is verified: first commit/tag `v0.1.0`, create the GitHub repo `yumeyis/KKVR_HandyPoser`, publish a release.

## Decisions log

| Date | Decision | Reason |
|---|---|---|
| 2026-11-10 | Target official VR only (KoikatuVR / Koikatsu Party VR) | Same environment as the KK_SetParentVR reference. Input layer abstracted for later expansion |
| 2026-11-10 | Reference game assemblies from the local install (`KKDir` or `lib/`), BepInEx/Harmony from NuGet | VR `Assembly-CSharp` is not published on NuGet. Game DLLs must not be committed |
| 2026-11-10 | CI = formatting check only | Full build needs proprietary DLLs |
| 2026-11-10 | MIT license, GUID `yumeyis.KKVR_HandyPoser` | Maintainer can change. GUID change would orphan user config files |
| 2026-11-10 | Default grab button = Grip, pins follow the character (`PinSpace.Character`) | Trigger is heavily used by the game UI. Character-relative pins survive character movement |
| 2026-11-10 | No code copied from KK_SetParentVR | It has no license |

## Session log

### 2026-11-10: pre-publish docs review (Claude Code)
- Reviewed docs/CI/.gitignore for the first GitHub publish. No code changed; nothing built.
- Release model: the DLL is NOT committed (`bin/` is git-ignored). CI cannot build (proprietary game DLLs), so the maintainer builds `dotnet build -c Release` locally and uploads `dist/KKVR_HandyPoser_v0.1.0.zip` to a GitHub Release.
- Open before publishing: set git identity, first commit, fix CHANGELOG `0.1.0 - Unreleased` date, adjust README "not verified" warning, tick STATUS checklist from the maintainer's in-game results.

### 2026-11-10: initial scaffolding (Claude Code, Linux, no game / no SDK)
- Created repo layout, MSBuild setup (`Directory.Build.props/targets`, NuGet.Config), docs, GitHub templates, lint CI.
- Implemented v0.1.0 feature set (grab / pin / reset, markers, haptics, input blocking, config).
- `git init` done (branch `main`). **No commit yet:** global git identity was not configured on that machine.
- Nothing compiled or tested.

### 2026-11-10: zh-TW config docs (Claude Code)
- Maintainer reported the plugin works in-game (not independently verified by the agent).
- `README.zh-TW.md` "設定" section now has a full settings table (same keys/defaults as `README.md`, including the keyboard shortcuts) instead of linking to the English README.

### 2026-11-10: marker default + Reset all buttons combo (Claude Code)
- "Show markers" default changed to `false` (existing cfg files keep their old value).
- Added `Reset all buttons` config (`VRButtonMask` flags, default `None`): resets all parts when the ticked buttons are held together on one controller (`HandyPoserController.IsResetComboPressed`).
- Verified: maintainer tested in-game and reported it works (Grip + Touchpad combo). Agent could not build (no dotnet / game DLLs).
- Known quirk: a combo that includes the grab button may grab a nearby part first, then reset releases it.
