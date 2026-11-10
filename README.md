# KKVR_HandyPoser

> Grab and pose characters' body parts directly with your VR controllers in **Koikatsu official VR**.

[繁體中文說明](README.zh-TW.md)

> [!WARNING]
> Early release (v0.1.0). Tested only on a limited setup; expect rough edges and please report issues.

## Features

- **Grab any body part** of any character in the H scene: hands, feet, elbows, knees, shoulders, thighs, hips.
- **Hold to drag, release to pin.** The part stays where you let go.
- **Double-click to reset** a pinned part back to the animation.
- Works on **all characters** in the scene, including the second girl in 3P and (optionally) the male.
- Markers show which parts are grabbable (white = free, cyan = pinned, yellow = hovered, green = grabbed).
- Pinned parts follow the character (default) or stay fixed in the room.
- Blocks the game's own action on the grab button while you are posing.

## Requirements

| | |
|---|---|
| Game | Koikatsu Party (Steam) **or** Koikatu (JP), with the official VR executable (`Koikatsu Party VR.exe` / `KoikatuVR.exe`) |
| Mod loader | [BepInEx 5.4.x](https://github.com/BepInEx/BepInEx) + HarmonyX. Both included in [HF Patch](https://github.com/ManlyMarco/KK-HF_Patch) |
| VR | Any SteamVR-compatible headset with two motion controllers |

## Installation

1. Download `KKVR_HandyPoser_vX.Y.Z.zip` from [Releases](https://github.com/yumeyis/KKVR_HandyPoser/releases).
2. Extract it into the game folder (the one with the `BepInEx` folder).
   The DLL should end up at `BepInEx/plugins/KKVR_HandyPoser/KKVR_HandyPoser.dll`.

## Usage

| Action | Input (default) |
|---|---|
| Grab a body part | Move a controller close to the part (marker turns yellow), **hold Grip** |
| Pin it in place | **Release Grip** |
| Move a pinned part again | Grab it again |
| Reset a part to the animation | **Double-click Grip** near the part |
| Reset everything | Keyboard shortcut (configure in settings) |
| Toggle posing on/off | Keyboard shortcut (configure in settings) |

Changing the H position / H point resets all parts automatically.

## Configuration

Press **F1** in the main game (outside VR) to open the ConfigurationManager, or edit
`BepInEx/config/yumeyis.KKVR_HandyPoser.cfg`.

| Setting | Default | Description |
|---|---|---|
| Enabled on start | `true` | Posing active when an H scene starts |
| Grab button | `Grip` | Grip / Trigger / Touchpad / Menu |
| Grab radius | `0.08` | Max distance (m) between controller and part |
| Double-click time | `0.3` | Seconds between clicks for a reset |
| Pin space | `Character` | `Character`: pins follow the character. `World`: pins stay in the room |
| Grabbable parts | `All` | Hands/feet, elbows/knees, shoulders/thighs, hips |
| Allow posing male | `true` | Include male characters |
| Block game input while posing | `true` | Hide the grab button from the game while posing |
| Haptic feedback | `true` | Vibrate on grab / release / reset |
| Show markers | `false` | Spheres on grabbable parts |
| Marker size / show distance | `0.03` / `0.4` | Marker diameter and appearance distance (m) |
| Toggle posing | unset | Keyboard shortcut to turn posing on/off |
| Reset all parts | unset | Keyboard shortcut to reset every pinned part |
| Reset all buttons | `None` | Button combo held together on one controller to reset everything (e.g. tick Grip + Touchpad). `None` disables it |

## Compatibility

- **KK_SetParentVR**: both plugins drive the same IK effectors. Using both on the same limb at the same time is not supported. Disable the limb features of one of them.
- **KoikatuVRAssistPlugin**: if it uses the same button as *Grab button*, change one of them.
- Not supported: the unofficial main-game VR mod (KK_MainGameVR) and CharaStudio VR. The input layer is abstracted, so support could be added later.

## Building from source

See [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md).

## Credits

- Inspired by [KK_SetParentVR](https://github.com/MayouKurayami/KK_SetParentVR) (MayouKurayami and the original anonymous author). No code was copied.
- Built on [BepInEx](https://github.com/BepInEx/BepInEx) and [HarmonyX](https://github.com/BepInEx/HarmonyX).

## License

[MIT](LICENSE)
