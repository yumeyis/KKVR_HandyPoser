using HarmonyLib;
using KKVR_HandyPoser.Core;

namespace KKVR_HandyPoser.Patches
{
    /// <summary>
    /// Hides the grab button from the game while a controller is posing, so grabbing a
    /// hand does not also trigger the game's own action bound to the same button.
    /// </summary>
    internal static class ControllerInputHooks
    {
        /// <summary>Set while HandyPoser itself reads input through the game API.</summary>
        internal static bool Bypass;

        // __0 = first argument (the button kind). Positional injection avoids depending on
        // the original parameter name in the game assembly.

        [HarmonyPrefix]
        [HarmonyPatch(typeof(VRViveController), nameof(VRViveController.IsState))]
        private static bool IsStatePrefix(VRViveController __instance, VRViveController.EViveButtonKind __0, ref bool __result)
            => Filter(__instance, __0, ref __result);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(VRViveController), nameof(VRViveController.IsPressDown))]
        private static bool IsPressDownPrefix(VRViveController __instance, VRViveController.EViveButtonKind __0, ref bool __result)
            => Filter(__instance, __0, ref __result);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(VRViveController), nameof(VRViveController.IsPressUp))]
        private static bool IsPressUpPrefix(VRViveController __instance, VRViveController.EViveButtonKind __0, ref bool __result)
            => Filter(__instance, __0, ref __result);

        /// <returns>false to skip the original method (input blocked).</returns>
        private static bool Filter(VRViveController controller, VRViveController.EViveButtonKind key, ref bool result)
        {
            if (Bypass)
                return true;

            var poser = HandyPoserController.Instance;
            if (poser == null || !poser.ShouldBlockGameInput(controller, key))
                return true;

            result = false;
            return false;
        }
    }
}
