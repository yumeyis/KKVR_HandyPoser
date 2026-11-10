using UnityEngine;

namespace KKVR_HandyPoser.Controls
{
    public enum ControllerSide
    {
        Left = 0,
        Right = 1,
    }

    public enum VRButton
    {
        Grip,
        Trigger,
        Touchpad,
        Menu,
    }

    /// <summary>Set of buttons for combo bindings (e.g. Grip + Touchpad).</summary>
    [System.Flags]
    public enum VRButtonMask
    {
        None = 0,
        Grip = 1 << 0,
        Trigger = 1 << 1,
        Touchpad = 1 << 2,
        Menu = 1 << 3,
    }

    /// <summary>
    /// Abstraction over a VR runtime's controllers, so the posing logic does not
    /// depend on a specific VR implementation (official VR today; VRGIN-based
    /// mods such as KK_MainGameVR could be added later).
    /// </summary>
    internal interface IControllerInput
    {
        /// <summary>Poll the hardware. Call exactly once per frame before any query.</summary>
        void Update();

        bool IsAvailable(ControllerSide side);

        /// <summary>Tracked transform of the controller (null if not available).</summary>
        Transform GetTransform(ControllerSide side);

        bool GetButton(ControllerSide side, VRButton button);

        bool GetButtonDown(ControllerSide side, VRButton button);

        bool GetButtonUp(ControllerSide side, VRButton button);

        void Haptic(ControllerSide side, ushort microseconds);
    }
}
