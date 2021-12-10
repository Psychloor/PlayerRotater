namespace PlayerRotater
{

    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;

    using MelonLoader;

    using UnhollowerRuntimeLib.XrefScans;

    using UnityEngine;

    internal static class ModPatches
    {

        private static OnLeftRoom origOnLeftRoom;


        private static ApplyPlayerMotion origApplyPlayerMotion;

        private static void ApplyPlayerMotionPatch(Vector3 playerWorldMotion, Quaternion playerWorldRotation)
        {
            origApplyPlayerMotion(playerWorldMotion, RotationSystem.Rotating ? Quaternion.identity : playerWorldRotation);
        }


        private static void OnLeftRoomPatch(IntPtr instancePtr)
        {
            RotationSystem.Instance.OnLeftWorld();
            origOnLeftRoom(instancePtr);
        }

        internal static bool PatchMethods()
        {
            try
            {
                // Left room
                MethodInfo onLeftRoomMethod = typeof(NetworkManager).GetMethod(
                    nameof(NetworkManager.OnLeftRoom),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                    null,
                    Type.EmptyTypes,
                    null);
                origOnLeftRoom = Patch<OnLeftRoom>(onLeftRoomMethod, GetDetour(nameof(OnLeftRoomPatch)));
            }
            catch (Exception e)
            {
                Utilities.LoggerInstance.Error("Failed to patch OnLeftRoom\n" + e.Message);
                return false;
            }

            if (Utilities.IsInVR)
                try
                {
                    // Fixes spinning issue
                    // TL;DR Prevents the tracking manager from applying rotational force
                    MethodInfo applyPlayerMotionMethod = typeof(VRCTrackingManager).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                                                   .Where(
                                                                                       m => m.Name.StartsWith("Method_Public_Static_Void_Vector3_Quaternion")
                                                                                            && !m.Name.Contains("_PDM_")).First(
                                                                                       m => XrefScanner.UsedBy(m).Any(
                                                                                           xrefInstance => xrefInstance.Type == XrefType.Method
                                                                                                           && xrefInstance.TryResolve()?.ReflectedType
                                                                                                                          ?.Equals(typeof(VRC_StationInternal))
                                                                                                           == true));
                    origApplyPlayerMotion = Patch<ApplyPlayerMotion>(applyPlayerMotionMethod, GetDetour(nameof(ApplyPlayerMotionPatch)));
                }
                catch (Exception e)
                {
                    Utilities.LoggerInstance.Error("Failed to patch ApplyPlayerMotion\n" + e.Message);
                    return false;
                }

            return true;
        }

        private static unsafe TDelegate Patch<TDelegate>(MethodBase originalMethod, IntPtr patchDetour)
        {
            Debug.Assert(typeof(TDelegate).GetCustomAttribute<UnmanagedFunctionPointerAttribute>() != null
                         && typeof(TDelegate).GetCustomAttribute<UnmanagedFunctionPointerAttribute>().CallingConvention
                         == CallingConvention.Cdecl, "You donkey, you fucked up the Native Delegate not having the right attribute");

            IntPtr original = *(IntPtr*)UnhollowerSupport.MethodBaseToIl2CppMethodInfoPointer(originalMethod);
            MelonUtils.NativeHookAttach((IntPtr)(&original), patchDetour);
            return Marshal.GetDelegateForFunctionPointer<TDelegate>(original);
        }

        private static IntPtr GetDetour(string name)
        {
            return typeof(ModPatches).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!.MethodHandle.GetFunctionPointer();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void OnLeftRoom(IntPtr instancePtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ApplyPlayerMotion(Vector3 playerWorldMotion, Quaternion playerWorldRotation);

    }

}