using System;
using System.Collections.Generic;
using HarmonyLib;
using KKVR_HandyPoser.Controls;
using KKVR_HandyPoser.Visual;
using UnityEngine;

namespace KKVR_HandyPoser.Core
{
    /// <summary>
    /// Per-H-scene component that turns controller input into body part grabs.
    /// Attached to the VRHScene object by <see cref="Patches.HSceneHooks"/>.
    /// </summary>
    internal sealed class HandyPoserController : MonoBehaviour
    {
        private const float RescanInterval = 2f;
        private const ushort HapticGrab = 1500;
        private const ushort HapticRelease = 800;
        private const ushort HapticReset = 3000;

        private readonly List<PoseTarget> _targets = new List<PoseTarget>();
        private readonly SideState[] _sides = { new SideState(), new SideState() };

        private OfficialVRControllerInput _input;
        private MarkerRenderer _markers;
        private List<MotionIK> _motionIKs;
        private HFlag _flags;
        private float _nextRescan;

        public static HandyPoserController Instance { get; private set; }

        public bool PosingEnabled { get; private set; }

        public static void Attach(VRHScene scene)
        {
            if (Instance != null)
                Destroy(Instance);

            Instance = scene.gameObject.AddComponent<HandyPoserController>();
            Instance._motionIKs = Traverse.Create(scene).Field("lstMotionIK").GetValue<List<MotionIK>>();
            var sprites = scene.sprites;
            Instance._flags = sprites != null && sprites.Length > 0 && sprites[0] != null ? sprites[0].flags : null;

            HandyPoserPlugin.Log.LogDebug("Attached to VRHScene");
        }

        private void Awake()
        {
            _input = new OfficialVRControllerInput();
            _markers = new MarkerRenderer();
            PosingEnabled = HandyPoserPlugin.EnabledOnStart.Value;
        }

        private void Update()
        {
            if (HandyPoserPlugin.ToggleKey.Value.IsDown())
                SetPosingEnabled(!PosingEnabled);
            if (HandyPoserPlugin.ResetAllKey.Value.IsDown())
                ResetAll();

            _input.Update();

            if (IsResetComboPressed())
                ResetAll();

            if (Time.unscaledTime >= _nextRescan)
                RescanTargets();

            for (int i = 0; i < _sides.Length; i++)
            {
                if (PosingEnabled)
                    UpdateSide((ControllerSide)i);
                else
                    _sides[i].Hover = null;
            }
        }

        /// <summary>True on the frame a configured button combo becomes fully held on one controller.</summary>
        private bool IsResetComboPressed()
        {
            var mask = HandyPoserPlugin.ResetAllButtons.Value;
            if (mask == VRButtonMask.None)
                return false;

            for (int s = 0; s < 2; s++)
            {
                var side = (ControllerSide)s;
                bool allHeld = true;
                bool anyDown = false;
                for (int b = 0; b < 4; b++)
                {
                    if (((int)mask & (1 << b)) == 0)
                        continue;
                    var button = (VRButton)b;
                    allHeld &= _input.GetButton(side, button);
                    anyDown |= _input.GetButtonDown(side, button);
                }
                if (allHeld && anyDown)
                    return true;
            }
            return false;
        }

        private void LateUpdate()
        {
            // The game's MotionIK may overwrite effector targets/weights on animation changes,
            // so the overrides are re-applied every frame.
            foreach (var target in _targets)
            {
                if (!target.IsValid)
                    continue;
                foreach (var handle in target.Handles)
                    handle.Apply();
            }

            _markers.Update(_targets, _input, _sides[0].Hover, _sides[1].Hover, PosingEnabled);
        }

        private void OnDestroy()
        {
            ResetAll();
            _markers.Dispose();
            if (Instance == this)
                Instance = null;
        }

        // ------------------------------------------------------------------
        // Public API (used by Harmony hooks)
        // ------------------------------------------------------------------

        public void SetPosingEnabled(bool enable)
        {
            if (!enable)
            {
                // Let go of anything being held, but keep pinned parts where they are.
                for (int i = 0; i < _sides.Length; i++)
                    ReleaseGrab((ControllerSide)i, haptic: false);
            }
            PosingEnabled = enable;
            HandyPoserPlugin.Log.LogMessage(enable ? "HandyPoser enabled" : "HandyPoser disabled");
        }

        /// <summary>Return every body part of every character to the animation.</summary>
        public void ResetAll()
        {
            foreach (var side in _sides)
            {
                side.Grabbed = null;
                side.Hover = null;
            }

            foreach (var target in _targets)
            {
                if (target.IsValid)
                    target.ResetAll();
            }
            RefreshMotionIK();
        }

        /// <summary>
        /// Whether the game should ignore <paramref name="kind"/> on <paramref name="controller"/>
        /// because the player is using it to pose.
        /// </summary>
        public bool ShouldBlockGameInput(VRViveController controller, VRViveController.EViveButtonKind kind)
        {
            if (!PosingEnabled || !HandyPoserPlugin.BlockGameInput.Value)
                return false;
            if (kind != OfficialVRControllerInput.ToKind(HandyPoserPlugin.GrabButton.Value))
                return false;
            if (!_input.TryGetSide(controller, out var side))
                return false;

            var state = _sides[(int)side];
            return state.Hover != null || state.Grabbed != null;
        }

        // ------------------------------------------------------------------
        // Internals
        // ------------------------------------------------------------------

        private void UpdateSide(ControllerSide side)
        {
            var state = _sides[(int)side];
            var controller = _input.GetTransform(side);
            if (controller == null)
            {
                ReleaseGrab(side, haptic: false);
                state.Hover = null;
                return;
            }

            var button = HandyPoserPlugin.GrabButton.Value;

            if (state.Grabbed != null)
            {
                // The part may have been reset from elsewhere (hotkey, scene hook).
                if (!state.Grabbed.IsGrabbed || !state.Grabbed.Owner.IsValid)
                    state.Grabbed = null;
                else if (!_input.GetButton(side, button))
                    ReleaseGrab(side, haptic: true);

                state.Hover = state.Grabbed;
                return;
            }

            state.Hover = FindNearest(controller, HandyPoserPlugin.GrabRadius.Value);
            if (state.Hover == null || !_input.GetButtonDown(side, button))
                return;

            float now = Time.unscaledTime;
            bool doubleClick = state.LastPressHandle == state.Hover
                && now - state.LastPressTime <= HandyPoserPlugin.DoubleClickTime.Value;
            state.LastPressTime = now;
            state.LastPressHandle = state.Hover;

            if (doubleClick && state.Hover.IsActive)
            {
                state.Hover.Reset();
                state.LastPressHandle = null;
                RefreshMotionIK();
                Haptic(side, HapticReset);
            }
            else
            {
                state.Hover.Grab(controller);
                state.Grabbed = state.Hover;
                Haptic(side, HapticGrab);
            }
        }

        private void ReleaseGrab(ControllerSide side, bool haptic)
        {
            var state = _sides[(int)side];
            var handle = state.Grabbed;
            state.Grabbed = null;
            if (handle == null || !handle.Owner.IsValid)
                return;

            var pinParent = HandyPoserPlugin.PinSpaceMode.Value == PinSpace.Character ? handle.Owner.PinRoot : null;
            handle.Release(pinParent);
            if (haptic)
                Haptic(side, HapticRelease);
        }

        private BodyPartHandle FindNearest(Transform controller, float radius)
        {
            BodyPartHandle best = null;
            float bestSqr = radius * radius;
            Vector3 pos = controller.position;
            var parts = HandyPoserPlugin.EnabledParts.Value;
            bool allowMale = HandyPoserPlugin.AllowMaleTargets.Value;

            foreach (var target in _targets)
            {
                if (!target.IsValid || !target.IsVisible || (target.IsMale && !allowMale))
                    continue;

                foreach (var handle in target.Handles)
                {
                    if (!parts.Contains(handle.Part.Group()))
                        continue;
                    // Do not steal a part held by the other controller.
                    if (handle.IsGrabbed && handle.GrabbedBy != controller)
                        continue;

                    float sqr = (handle.Bone.position - pos).sqrMagnitude;
                    if (sqr < bestSqr)
                    {
                        bestSqr = sqr;
                        best = handle;
                    }
                }
            }
            return best;
        }

        private void RescanTargets()
        {
            _nextRescan = Time.unscaledTime + RescanInterval;
            _targets.RemoveAll(t => !t.IsValid);

            foreach (var character in FindObjectsOfType<ChaControl>())
            {
                if (_targets.Exists(t => t.Character == character))
                    continue;

                var target = PoseTarget.TryCreate(character);
                if (target != null)
                {
                    _targets.Add(target);
                    HandyPoserPlugin.Log.LogDebug($"Tracking {target.Name} ({target.Handles.Count} parts)");
                }
            }
        }

        /// <summary>
        /// Ask the game to recompute its animation-driven IK. Restoring an effector's
        /// original target after a motion change can leave its BaseData bone unset;
        /// recalculating MotionIK for the current state fixes that.
        /// </summary>
        private void RefreshMotionIK()
        {
            if (_motionIKs == null || _flags == null)
                return;
            try
            {
                foreach (var motionIK in _motionIKs)
                    motionIK?.Calc(_flags.nowAnimStateName);
            }
            catch (Exception ex)
            {
                HandyPoserPlugin.Log.LogWarning($"MotionIK refresh failed: {ex.Message}");
            }
        }

        private void Haptic(ControllerSide side, ushort microseconds)
        {
            if (HandyPoserPlugin.HapticFeedback.Value)
                _input.Haptic(side, microseconds);
        }

        internal sealed class SideState
        {
            public BodyPartHandle Hover;
            public BodyPartHandle Grabbed;
            public BodyPartHandle LastPressHandle;
            public float LastPressTime = -10f;
        }
    }
}
