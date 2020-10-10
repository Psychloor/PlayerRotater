namespace PlayerRotater
{

    using System;
    using System.Reflection;

    using Harmony;

    using MelonLoader;

    internal static class ModPatches
    {

        public static void Patch(HarmonyInstance instance)
        {
            try
            {
                // Left room
                instance.Patch(
                    typeof(NetworkManager).GetMethod(nameof(NetworkManager.OnLeftRoom), BindingFlags.Public | BindingFlags.Instance),
                    null,
                    GetPatch(nameof(LeftWorldPatch)));
            }
            catch (Exception e)
            {
                MelonLogger.LogWarning("Failed to patch OnLeftRoom\n" + e.Message);
            }

            try
            {
                // Faded to and joined and initialized room
                instance.Patch(
                    typeof(VRCUiManager).GetMethod(nameof(VRCUiManager.Method_Public_Void_String_Single_Action_0)),
                    null,
                    GetPatch(nameof(JoinedRoomPatch)));
            }
            catch (Exception e)
            {
                MelonLogger.LogWarning("Failed to patch FadeTo Initialized room\n" + e.Message);
            }
        }

        private static void LeftWorldPatch()
        {
            RotationSystem.Instance.OnLeftWorld();
        }

        private static void JoinedRoomPatch(string __0, float __1)
        {
            if (__0.Equals("BlackFade")
                && __1.Equals(0f)
                && RoomManagerBase.field_Internal_Static_ApiWorldInstance_0 != null)
                MelonCoroutines.Start(Utilities.CheckWorld());
        }

        private static HarmonyMethod GetPatch(string name)
        {
            return new HarmonyMethod(typeof(ModPatches).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static));
        }

    }

}