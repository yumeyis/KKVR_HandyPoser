# lib/

Fallback location for the game assemblies the plugin compiles against.
**DLLs placed here are git-ignored and must never be committed** (they are proprietary game files).

You only need this folder if you do **not** set `KKDir` in `Directory.Build.user.props`.

Required files, copied from `<game>\Koikatsu Party VR_Data\Managed\` (Steam) or `<game>\KoikatuVR_Data\Managed\` (JP release):

| File | Why |
|---|---|
| `Assembly-CSharp.dll` | `VRHScene`, `VRViveController`, `ChaControl`, `MotionIK`, ... (the **VR** build, not the main game one) |
| `Assembly-CSharp-firstpass.dll` | SteamVR plugin, RootMotion FinalIK |
| `UnityEngine.dll` | Unity 5.6 engine |
| `UnityEngine.UI.dll` | Unity UI |

Use `tools/copy-game-libs.ps1` (Windows) or `tools/copy-game-libs.sh` (Linux/WSL) to copy them automatically.
