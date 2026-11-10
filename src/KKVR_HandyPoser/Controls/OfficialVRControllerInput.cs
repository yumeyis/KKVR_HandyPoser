using System;
using System.Reflection;
using HarmonyLib;
using KKVR_HandyPoser.Patches;
using UnityEngine;

namespace KKVR_HandyPoser.Controls
{
    /// <summary>
    /// Controller input for the official Koikatsu VR executable
    /// (KoikatuVR.exe / Koikatsu Party VR.exe).
    ///
    /// Reads buttons straight from the SteamVR device held by the game's
    /// <see cref="VRViveController"/>, falling back to the game's own
    /// <c>IsState</c> API when the device is not yet assigned.
    /// </summary>
    internal sealed class OfficialVRControllerInput : IControllerInput
    {
        private const float RescanInterval = 1f;

        private static readonly FieldInfo DeviceField = AccessTools.Field(typeof(VRViveController), "device");

        private readonly VRViveController[] _controllers = new VRViveController[2];
        private readonly bool[,] _current = new bool[2, 4];
        private readonly bool[,] _previous = new bool[2, 4];
        private float _nextRescan;

        public bool IsAvailable(ControllerSide side) => _controllers[(int)side] != null;

        public Transform GetTransform(ControllerSide side)
        {
            // Explicit Unity null check: `?.` would skip UnityEngine.Object's destroyed-object check.
            var controller = _controllers[(int)side];
            return controller != null ? controller.transform : null;
        }

        /// <summary>Returns which side a game controller component belongs to.</summary>
        public bool TryGetSide(VRViveController controller, out ControllerSide side)
        {
            for (int i = 0; i < 2; i++)
            {
                if (_controllers[i] == controller)
                {
                    side = (ControllerSide)i;
                    return true;
                }
            }
            side = default;
            return false;
        }

        public void Update()
        {
            if ((_controllers[0] == null || _controllers[1] == null) && Time.unscaledTime >= _nextRescan)
            {
                _nextRescan = Time.unscaledTime + RescanInterval;
                FindControllers();
            }

            for (int s = 0; s < 2; s++)
            {
                for (int b = 0; b < 4; b++)
                {
                    _previous[s, b] = _current[s, b];
                    _current[s, b] = _controllers[s] != null && ReadButton(_controllers[s], (VRButton)b);
                }
            }
        }

        public bool GetButton(ControllerSide side, VRButton button) => _current[(int)side, (int)button];

        public bool GetButtonDown(ControllerSide side, VRButton button) =>
            _current[(int)side, (int)button] && !_previous[(int)side, (int)button];

        public bool GetButtonUp(ControllerSide side, VRButton button) =>
            !_current[(int)side, (int)button] && _previous[(int)side, (int)button];

        public void Haptic(ControllerSide side, ushort microseconds)
        {
            var controller = _controllers[(int)side];
            if (controller == null)
                return;
            GetDevice(controller)?.TriggerHapticPulse(microseconds, Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
        }

        private void FindControllers()
        {
            foreach (var controller in UnityEngine.Object.FindObjectsOfType<VRViveController>())
            {
                string name = controller.name.ToLowerInvariant();
                if (name.Contains("left"))
                    _controllers[(int)ControllerSide.Left] = controller;
                else if (name.Contains("right"))
                    _controllers[(int)ControllerSide.Right] = controller;
            }
        }

        private static SteamVR_Controller.Device GetDevice(VRViveController controller)
        {
            return DeviceField?.GetValue(controller) as SteamVR_Controller.Device;
        }

        private static bool ReadButton(VRViveController controller, VRButton button)
        {
            var device = GetDevice(controller);
            if (device != null)
                return device.GetPress(ToMask(button));

            // Fallback to the game's API. Our own Harmony prefix must not block this call.
            ControllerInputHooks.Bypass = true;
            try
            {
                return controller.IsState(ToKind(button), -1);
            }
            finally
            {
                ControllerInputHooks.Bypass = false;
            }
        }

        private static ulong ToMask(VRButton button)
        {
            switch (button)
            {
                case VRButton.Grip: return SteamVR_Controller.ButtonMask.Grip;
                case VRButton.Trigger: return SteamVR_Controller.ButtonMask.Trigger;
                case VRButton.Touchpad: return SteamVR_Controller.ButtonMask.Touchpad;
                case VRButton.Menu: return SteamVR_Controller.ButtonMask.ApplicationMenu;
                default: throw new ArgumentOutOfRangeException(nameof(button));
            }
        }

        internal static VRViveController.EViveButtonKind ToKind(VRButton button)
        {
            switch (button)
            {
                case VRButton.Grip: return VRViveController.EViveButtonKind.Grip;
                case VRButton.Trigger: return VRViveController.EViveButtonKind.Trigger;
                case VRButton.Touchpad: return VRViveController.EViveButtonKind.Touchpad;
                case VRButton.Menu: return VRViveController.EViveButtonKind.Menu;
                default: throw new ArgumentOutOfRangeException(nameof(button));
            }
        }
    }
}
