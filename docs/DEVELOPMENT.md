# Development guide

## Prerequisites

- **.NET SDK 8.0+** (any modern SDK can build `net35` thanks to the
  `Microsoft.NETFramework.ReferenceAssemblies` package). Visual Studio 2022 or Rider also work.
- A Koikatsu install **with the official VR executable** and **HF Patch** (BepInEx 5.4.x).
- Optional but very useful (all included in HF Patch):
  - **ConfigurationManager**: F1 settings window (main game only).
  - **RuntimeUnityEditor**: F12 inspector, browse the scene hierarchy and live values.
  - **dnSpyEx / ILSpy**: read `Assembly-CSharp.dll` of the VR build to find hook points.

## 1. Point the build at your game

The VR build ships its own `Assembly-CSharp.dll` (with `VRHScene`, `VRViveController`, SteamVR, ...).
It is not on NuGet, so the project references it from your install. Pick **one** option:

**Option A: `Directory.Build.user.props` (recommended)**

```bash
cp Directory.Build.user.props.example Directory.Build.user.props
# edit KKDir to point at your game folder
```

With `KKDir` set, every build also copies the DLL to `<KKDir>\BepInEx\plugins\KKVR_HandyPoser\`.

**Option B: copy the DLLs into `lib/`**

```powershell
./tools/copy-game-libs.ps1 -GameDir "C:\Illusion\Koikatsu Party"
```
```bash
tools/copy-game-libs.sh "/path/to/Koikatsu Party"
```

`lib/*.dll` is git-ignored. **Never commit game assemblies.**

## 2. Build

```bash
dotnet build                 # Debug  -> bin/Debug/KKVR_HandyPoser.dll
dotnet build -c Release      # Release -> bin/Release/ + dist/KKVR_HandyPoser_v<version>.zip
```

## 3. Test in game

1. Build (auto-deploys with Option A, otherwise copy `bin/Debug/KKVR_HandyPoser.dll` to
   `BepInEx/plugins/KKVR_HandyPoser/`).
2. Enable the BepInEx console: `BepInEx/config/BepInEx.cfg` → `[Logging.Console] Enabled = true`.
   For debug messages also set `[Logging.Console] LogLevels = All`.
3. Start the VR executable and enter an H scene. You should see `Attached to VRHScene` and
   `Tracking <character> (13 parts)` in the console.
4. Logs are also written to `BepInEx/LogOutput.log`.

## Project layout

```
KKVR_HandyPoser/
├─ src/KKVR_HandyPoser/
│  ├─ HandyPoserPlugin.cs      BepInEx entry point, config, Harmony setup
│  ├─ Core/                    Posing logic (VR-runtime independent except the controller)
│  │  ├─ HandyPoserController  Per-H-scene MonoBehaviour: input → grab/release/reset
│  │  ├─ PoseTarget            A character + its grabbable parts
│  │  ├─ BodyPartHandle        One part: FBBIK effector or bend goal
│  │  └─ Enums                 BodyPart, BodyPartGroup, PinSpace
│  ├─ Controls/                VR input abstraction + official VR implementation
│  ├─ Patches/                 Harmony hooks into VRHScene / VRViveController
│  └─ Visual/                  Grab markers
├─ docs/                       Developer documentation
├─ tools/                      Helper scripts
├─ lib/                        (git-ignored DLLs) fallback reference location
├─ reference/                  (git-ignored) third-party source used for research
├─ Directory.Build.props       Shared MSBuild settings, version number, game path resolution
└─ Directory.Build.targets     Deploy-to-game and release-zip targets
```

See [ARCHITECTURE.md](ARCHITECTURE.md) for how the pieces fit together, and
[STATUS.md](STATUS.md) for current progress and the verification checklist.

## Releasing

1. Bump `<Version>` in `Directory.Build.props` **and** `HandyPoserPlugin.Version`.
2. Move the `Unreleased` entries in `CHANGELOG.md` under the new version.
3. `dotnet build -c Release` → upload `dist/KKVR_HandyPoser_v<version>.zip` to a GitHub release
   tagged `v<version>`.

## CI

GitHub Actions only checks formatting (`dotnet format whitespace`). A full build is not possible
in CI because the game assemblies cannot be stored in the repository.
