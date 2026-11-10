using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KKVR_HandyPoser.Core;
using KKVR_HandyPoser.Controls;
using KKVR_HandyPoser.Patches;

namespace KKVR_HandyPoser
{
    /// <summary>
    /// BepInEx entry point. Only loads inside the official VR executables.
    /// </summary>
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInProcess("KoikatuVR")]
    [BepInProcess("Koikatsu Party VR")]
    public sealed class HandyPoserPlugin : BaseUnityPlugin
    {
        public const string GUID = "yumeyis.KKVR_HandyPoser";
        public const string PluginName = "KKVR HandyPoser";
        public const string Version = "0.1.0";

        internal static ManualLogSource Log { get; private set; }

        // ---- General ----
        internal static ConfigEntry<bool> EnabledOnStart { get; private set; }
        internal static ConfigEntry<VRButton> GrabButton { get; private set; }
        internal static ConfigEntry<float> GrabRadius { get; private set; }
        internal static ConfigEntry<float> DoubleClickTime { get; private set; }
        internal static ConfigEntry<PinSpace> PinSpaceMode { get; private set; }
        internal static ConfigEntry<BodyPartGroup> EnabledParts { get; private set; }
        internal static ConfigEntry<bool> AllowMaleTargets { get; private set; }
        internal static ConfigEntry<bool> BlockGameInput { get; private set; }
        internal static ConfigEntry<bool> HapticFeedback { get; private set; }

        // ---- Markers ----
        internal static ConfigEntry<bool> ShowMarkers { get; private set; }
        internal static ConfigEntry<float> MarkerSize { get; private set; }
        internal static ConfigEntry<float> MarkerShowDistance { get; private set; }

        // ---- Keyboard ----
        internal static ConfigEntry<KeyboardShortcut> ToggleKey { get; private set; }
        internal static ConfigEntry<KeyboardShortcut> ResetAllKey { get; private set; }
        internal static ConfigEntry<VRButtonMask> ResetAllButtons { get; private set; }

        private void Awake()
        {
            Log = Logger;
            BindConfig();

            var harmony = new Harmony(GUID);
            try
            {
                harmony.PatchAll(typeof(HSceneHooks));
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to hook the VR H scene, plugin disabled.\n{ex}");
                harmony.UnpatchSelf();
                enabled = false;
                return;
            }

            try
            {
                harmony.PatchAll(typeof(ControllerInputHooks));
            }
            catch (Exception ex)
            {
                // Non-fatal: posing still works, the game just also sees the grab button.
                Log.LogWarning($"Failed to hook controller input; game input will not be blocked while posing.\n{ex}");
            }
        }

        private void BindConfig()
        {
            const string general = "General";
            EnabledOnStart = Config.Bind(general, "Enabled on start", true,
                "Whether posing is active when an H scene starts. Can be toggled with the keyboard shortcut.");
            GrabButton = Config.Bind(general, "Grab button", VRButton.Grip,
                "Controller button used to grab a body part. Hold to drag, release to pin. Double-click near a pinned part to reset it.");
            GrabRadius = Config.Bind(general, "Grab radius", 0.08f,
                new ConfigDescription("How close (meters) the controller must be to a body part to grab it.",
                    new AcceptableValueRange<float>(0.02f, 0.3f)));
            DoubleClickTime = Config.Bind(general, "Double-click time", 0.3f,
                new ConfigDescription("Max seconds between two presses to count as a double-click.",
                    new AcceptableValueRange<float>(0.1f, 1f)));
            PinSpaceMode = Config.Bind(general, "Pin space", PinSpace.Character,
                "Character: pinned parts follow the character when it moves. World: pinned parts stay fixed in the room.");
            EnabledParts = Config.Bind(general, "Grabbable parts", BodyPartGroup.All,
                "Which groups of body parts can be grabbed.");
            AllowMaleTargets = Config.Bind(general, "Allow posing male", true,
                "Allow grabbing body parts of male characters.");
            BlockGameInput = Config.Bind(general, "Block game input while posing", true,
                "Prevent the game from reacting to the grab button while a controller hovers over or holds a body part.");
            HapticFeedback = Config.Bind(general, "Haptic feedback", true,
                "Vibrate the controller when grabbing, releasing or resetting a part.");

            const string markers = "Markers";
            ShowMarkers = Config.Bind(markers, "Show markers", false,
                "Show small spheres on grabbable body parts when a controller is nearby.");
            MarkerSize = Config.Bind(markers, "Marker size", 0.03f,
                new ConfigDescription("Diameter of the markers in meters.", new AcceptableValueRange<float>(0.005f, 0.1f)));
            MarkerShowDistance = Config.Bind(markers, "Marker show distance", 0.4f,
                new ConfigDescription("Markers appear when a controller is within this distance (meters) of a part.",
                    new AcceptableValueRange<float>(0.05f, 2f)));

            const string keys = "Keyboard shortcuts";
            ToggleKey = Config.Bind(keys, "Toggle posing", KeyboardShortcut.Empty,
                "Enable/disable controller posing.");
            ResetAllKey = Config.Bind(keys, "Reset all parts", KeyboardShortcut.Empty,
                "Release every pinned body part back to the animation.");
            ResetAllButtons = Config.Bind(general, "Reset all buttons", VRButtonMask.None,
                "Controller button combo that resets all parts. Tick every button that must be held together on the same controller " +
                "(e.g. Grip + Touchpad). None disables it.");
        }
    }
}
