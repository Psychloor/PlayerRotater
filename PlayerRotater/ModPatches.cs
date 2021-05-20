namespace PlayerRotater
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;

    using MelonLoader;

    using UnhollowerBaseLib;

    using UnhollowerRuntimeLib.XrefScans;

    using UnityEngine;

    internal static class ModPatches
    {

        private static OnLeftRoom origOnLeftRoom;

        private static FadeTo origFadeTo;

        private static ApplyPlayerMotion origApplyPlayerMotion;

        private static void FadeToPatch(IntPtr instancePtr, IntPtr fadeNamePtr, float fade, IntPtr actionPtr, IntPtr stackPtr)
        {
            if (instancePtr == IntPtr.Zero) return;
            origFadeTo(instancePtr, fadeNamePtr, fade, actionPtr, stackPtr);

            if (!IL2CPP.Il2CppStringToManaged(fadeNamePtr).Equals("BlackFade", StringComparison.Ordinal)
                || !fade.Equals(0f)
                || RoomManager.field_Internal_Static_ApiWorldInstance_0 == null) return;

            MelonCoroutines.Start(Utilities.CheckWorld());
        }

        private static void OnLeftRoomPatch(IntPtr instancePtr)
        {
            if (instancePtr == IntPtr.Zero) return;
            RotationSystem.Instance.OnLeftWorld();
            origOnLeftRoom(instancePtr);
        }

        internal static bool Patch()
        {
            try
            {
                // Left room
                unsafe
                {
                    MethodInfo onLeftRoomMethod = typeof(NetworkManager).GetMethod(
                        nameof(NetworkManager.OnLeftRoom),
                        BindingFlags.Public | BindingFlags.Instance);

                    IntPtr originalMethod =
                        *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(onLeftRoomMethod).GetValue(null);

                    MelonUtils.NativeHookAttach((IntPtr)(&originalMethod), GetDetour(nameof(OnLeftRoomPatch)));
                    origOnLeftRoom = Marshal.GetDelegateForFunctionPointer<OnLeftRoom>(originalMethod);
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error("Failed to patch OnLeftRoom\n" + e.Message);
                return false;
            }

            try
            {
                unsafe
                {
                    // Faded to and joined and initialized room
                    IEnumerable<MethodInfo> fadeMethods = typeof(VRCUiManager).GetMethods().Where(
                        m => m.Name.StartsWith("Method_Public_Void_String_Single_Action_") && m.GetParameters().Length == 3);
                    foreach (IntPtr originalMethod in fadeMethods.Select(
                        fadeMethod => *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(fadeMethod).GetValue(null)))
                    {
                        MelonUtils.NativeHookAttach((IntPtr)(&originalMethod), GetDetour(nameof(FadeToPatch)));
                        origFadeTo = Marshal.GetDelegateForFunctionPointer<FadeTo>(originalMethod);
                    }
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error("Failed to patch FadeTo\n" + e.Message);
                return false;
            }

            if (Utilities.IsInVR)
                try
                {
                    unsafe
                    {
                        // Fixes spinning issue
                        // TL;DR Prevents the tracking manager from applying rotational force
                        MethodInfo applyPlayerMotionMethod = typeof(VRCTrackingManager).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                                                       .Where(
                                                                                           m => m.Name.StartsWith(
                                                                                                    "Method_Public_Static_Void_Vector3_Quaternion")
                                                                                                && !m.Name.Contains("_PDM_")).First(
                                                                                           m => XrefScanner.UsedBy(m).Any(
                                                                                               xrefInstance => xrefInstance.Type == XrefType.Method
                                                                                                               && xrefInstance.TryResolve()?.ReflectedType
                                                                                                                              ?.Equals(
                                                                                                                                  typeof(VRC_StationInternal))
                                                                                                               == true));
                        IntPtr originalMethod = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(applyPlayerMotionMethod)
                                                                                 .GetValue(null);

                        MelonUtils.NativeHookAttach((IntPtr)(&originalMethod), GetDetour(nameof(ApplyPlayerMotionPatch)));
                        origApplyPlayerMotion = Marshal.GetDelegateForFunctionPointer<ApplyPlayerMotion>(originalMethod);
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Error("Failed to patch ApplyPlayerMotion\n" + e.Message);
                    return false;
                }

            return true;
        }

        private static void ApplyPlayerMotionPatch(Vector3 playerWorldMotion, Quaternion playerWorldRotation)
        {
            origApplyPlayerMotion(playerWorldMotion, RotationSystem.Rotating ? Quaternion.identity : playerWorldRotation);
        }

        private static IntPtr GetDetour(string name)
        {
            return typeof(ModPatches).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!.MethodHandle.GetFunctionPointer();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void OnLeftRoom(IntPtr instancePtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FadeTo(IntPtr instancePtr, IntPtr fadeNamePtr, float fade, IntPtr actionPtr, IntPtr stackPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ApplyPlayerMotion(Vector3 playerWorldMotion, Quaternion playerWorldRotation);

    }

}