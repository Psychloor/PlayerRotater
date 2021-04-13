namespace PlayerRotater
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Harmony;

    using MelonLoader;

    using UnhollowerRuntimeLib.XrefScans;

    using UnityEngine;

    internal static class ModPatches
    {

        internal static void Patch(HarmonyInstance instance)
        {
            try
            {
                // Left room
                instance.Patch(
                    typeof(NetworkManager).GetMethod(nameof(NetworkManager.OnLeftRoom), BindingFlags.Public | BindingFlags.Instance),
                    postfix: GetPatch(nameof(LeftWorldPatch)));
            }
            catch (Exception e)
            {
                MelonLogger.Error("Failed to patch OnLeftRoom\n" + e.Message);
            }

            try
            {
                // Faded to and joined and initialized room
                IEnumerable<MethodInfo> fadeMethods = typeof(VRCUiManager).GetMethods()
                                                                          .Where(
                                                                              m => m.Name.StartsWith("Method_Public_Void_String_Single_Action_")
                                                                                   && m.GetParameters().Length == 3);
                foreach (MethodInfo fadeMethod in fadeMethods) instance.Patch(fadeMethod, postfix: GetPatch(nameof(JoinedRoomPatch)));
            }
            catch (Exception e)
            {
                MelonLogger.Error("Failed to patch FadeTo Initialized room\n" + e.Message);
            }

            if (Utilities.IsVR)
                try
                {
                    // Fixes spinning issue
                    // TL;DR Prevents the tracking manager from applying rotational force
                    instance.Patch(
                        typeof(VRCTrackingManager).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                  .Where(m => m.Name.StartsWith("Method_Public_Static_Void_Vector3_Quaternion") && !m.Name.Contains("_PDM_"))
                                                  .First(
                                                      m => XrefScanner.UsedBy(m).Any(
                                                          instance => instance.Type == XrefType.Method
                                                                      && instance.TryResolve()?.ReflectedType?.Equals(typeof(VRC_StationInternal)) == true)),
                        GetPatch(nameof(ApplyPlayerMotionPatch)));
                }
                catch (Exception e)
                {
                    MelonLogger.Error("Failed to patch ApplyPlayerMotion\n" + e.Message);
                }
        }

        private static void LeftWorldPatch()
        {
            Utilities.LogDebug("Left World Patch");
            RotationSystem.Instance.OnLeftWorld();
        }

        private static void JoinedRoomPatch(string __0, float __1)
        {
            Utilities.LogDebug("Joined Room Patch");
            if (__0.Equals("BlackFade")
                && __1.Equals(0f)
                && RoomManager.field_Internal_Static_ApiWorldInstance_0 != null)
                MelonCoroutines.Start(Utilities.CheckWorld());
        }

        private static void ApplyPlayerMotionPatch(ref Vector3 __0, ref Quaternion __1)
        {
            if (RotationSystem.Rotating) __1 = Quaternion.identity;
        }

        private static HarmonyMethod GetPatch(string name)
        {
            return new(typeof(ModPatches).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static));
        }

    }

}