using System;
using HarmonyLib;
using KKVR_HandyPoser.Core;

namespace KKVR_HandyPoser.Patches
{
    /// <summary>
    /// Hooks into the official VR H scene lifecycle.
    /// </summary>
    internal static class HSceneHooks
    {
        /// <summary>
        /// MapSameObjectDisable runs near the end of H scene loading, when characters,
        /// IK and controllers all exist. Commonly used by other KK plugins for the same reason.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VRHScene), "MapSameObjectDisable")]
        private static void OnHSceneLoaded(VRHScene __instance)
        {
            try
            {
                HandyPoserController.Attach(__instance);
            }
            catch (Exception ex)
            {
                HandyPoserPlugin.Log.LogError($"Failed to initialize in H scene: {ex}");
            }
        }

        /// <summary>
        /// Called when the H point / position changes and characters are teleported.
        /// Pinned anchors would be left behind, so release everything.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(VRHScene), "SetLocalPosition")]
        private static void OnHPointChanged()
        {
            var controller = HandyPoserController.Instance;
            if (controller != null)
                controller.ResetAll();
        }
    }
}
