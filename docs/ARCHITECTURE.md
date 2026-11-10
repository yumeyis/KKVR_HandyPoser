# Architecture

## Runtime flow

```
BepInEx loads HandyPoserPlugin (only in KoikatuVR / Koikatsu Party VR)
  └─ Harmony patches
       ├─ VRHScene.MapSameObjectDisable (postfix)  → HandyPoserController.Attach(scene)
       ├─ VRHScene.SetLocalPosition     (prefix)   → controller.ResetAll()
       └─ VRViveController.IsState/IsPressDown/IsPressUp (prefix)
                                                   → hide grab button from the game while posing

HandyPoserController (MonoBehaviour on the VRHScene object)
  Update()
    ├─ keyboard shortcuts (toggle, reset all)
    ├─ IControllerInput.Update()   poll buttons once per frame, compute down/up edges
    ├─ RescanTargets()             every 2 s: track new ChaControl, drop destroyed ones
    └─ per controller side:
         grabbing? → button released → Release() (pin)
         else      → FindNearest() within GrabRadius → hover
                     button down: double-click on active part → Reset()
                                  otherwise                   → Grab(controller)
  LateUpdate()
    ├─ BodyPartHandle.Apply() for every active part (re-assert IK overrides)
    └─ MarkerRenderer.Update()
```

## How a body part is moved

Koikatsu characters use **RootMotion FinalIK `FullBodyBipedIK`** on `ChaControl.objAnim`.
The game drives each effector's `target` from animation-controlled transforms (`cf_t_*`, with
`BaseData` components) and sets weights per animation via `MotionIK`.

`BodyPartHandle` takes over a part by creating an **anchor** GameObject and pointing the IK at it:

| Part | IK element | Override |
|---|---|---|
| Hands, feet | `IKEffector` | `target = anchor`, position + rotation weight 1 |
| Shoulders, thighs, hips | `IKEffector` | `target = anchor`, position weight 1 |
| Elbows, knees | `FBIKChain.bendConstraint` | `bendGoal = anchor`, weight 1 |

- **Grab**: anchor is parented to the controller (keeping the current offset, so no snapping).
- **Release**: anchor is re-parented to the character root (`PinSpace.Character`) or to nothing (`PinSpace.World`).
- **Reset**: the original target/weights are restored, the anchor is destroyed, and
  `MotionIK.Calc(nowAnimStateName)` is called to re-bind the game's IK (restoring a target after a
  motion change can otherwise leave its `BaseData.bone` null, a behavior also observed by KK_SetParentVR).

## Input abstraction

`IControllerInput` hides the VR runtime. `OfficialVRControllerInput` reads the SteamVR device that
the game's `VRViveController` holds in its private `device` field, falling back to
`VRViveController.IsState` (with `ControllerInputHooks.Bypass` so our own block does not apply).

To support another runtime (e.g. VRGIN-based KK_MainGameVR), implement `IControllerInput` and add a
matching scene hook. `Core/` should not need changes.

## Known open points

- **IK update order.** Overrides are applied in `LateUpdate`, and FinalIK also solves in `LateUpdate`,
  so the order is not guaranteed (possible one-frame lag). If this is noticeable, hook
  `IKSolver.OnPreUpdate` instead.
- **Markers use the default lit material.** They can be hidden inside the body. An always-on-top shader
  would need to be shipped in an AssetBundle.
- **Head / neck** are not FBBIK effectors. Posing them needs a separate approach (e.g. `NeckLookController`
  or a LookAt target).
- Hook targets (`MapSameObjectDisable`, `SetLocalPosition`, `VRViveController.*`) and the `device` field
  are taken from the VR build of `Assembly-CSharp`. Verify them with dnSpy after game updates.
