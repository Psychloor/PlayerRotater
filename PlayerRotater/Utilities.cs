namespace PlayerRotater
{

    using System;
    using System.Collections;
    using System.Linq;
    using System.Reflection;

    using MelonLoader;

    using UnhollowerRuntimeLib.XrefScans;

    using UnityEngine;

    using VRC.Core;

    internal static class Utilities
    {

        private static MethodInfo alignTrackingToPlayerMethod;

        // Yes that's a lot of xref scanning but gotta make sure xD
        // Only grabs once anyway ¯\_(ツ)_/¯
        internal static AlignTrackingToPlayerDelegate GetAlignTrackingToPlayerDelegate
        {
            get
            {
                alignTrackingToPlayerMethod ??= typeof(VRCPlayer).GetMethods(BindingFlags.Public | BindingFlags.Instance).First(
                    m => m.ReturnType == typeof(void) && m.GetParameters().Length == 0 && m.XRefScanForMethod("get_Transform")
                         && m.XRefScanForMethod(reflectedType: "Player") && m.XRefScanForMethod("Vector3_Quaternion", "VRCPlayer")
                         && m.XRefScanForMethod(reflectedType: "VRCTrackingManager") && m.XRefScanForMethod(reflectedType: "InputStateController"));
                
                return (AlignTrackingToPlayerDelegate)Delegate.CreateDelegate(
                    typeof(AlignTrackingToPlayerDelegate),
                    GetLocalVRCPlayer(),
                    alignTrackingToPlayerMethod);
            }
        }

        internal static IEnumerator CheckWorld()
        {
            // Hi KiraiChan, while removing this check too, add yourself as "author" too?
            // So it isn't just me in your unlocked version
            string worldId = RoomManagerBase.field_Internal_Static_ApiWorld_0.id;
            RotationSystem.Instance.WorldAllowed = false;

            // Check if black/whitelisted from EmmVRC - thanks Emilia and the rest of EmmVRC Staff
            WWW www = new WWW($"https://thetrueyoshifan.com/RiskyFuncsCheck.php?worldid={worldId}");
            while (!www.isDone)
                yield return new WaitForEndOfFrame();
            string result = www.text?.Trim().ToLower();
            www.Dispose();
            if (!string.IsNullOrWhiteSpace(result))
                switch (result)
                {
                    case "allowed":
                        RotationSystem.Instance.WorldAllowed = true;
                        yield break;

                    case "denied":
                        RotationSystem.Instance.WorldAllowed = false;
                        yield break;
                }

            // no result from server or they're currently down
            // Check tags then. should also be in cache as it just got downloaded
            API.Fetch<ApiWorld>(
                worldId,
                new Action<ApiContainer>(
                    container =>
                        {
                            ApiWorld apiWorld;
                            if ((apiWorld = container.Model.TryCast<ApiWorld>()) != null)
                            {
                                foreach (string worldTag in apiWorld.tags)
                                    if (worldTag.IndexOf("game", StringComparison.OrdinalIgnoreCase) >= 0)
                                        return;

                                RotationSystem.Instance.WorldAllowed = true;
                            }
                            else
                            {
                                MelonLogger.LogError("Failed to cast ApiModel to ApiWorld");
                            }
                        }),
                disableCache: false);
        }

        internal static VRCPlayer GetLocalVRCPlayer()
        {
            return VRCPlayer.field_Internal_Static_VRCPlayer_0;
        }

        private static bool XRefScanForMethod(this MethodBase methodBase, string methodName = null, string reflectedType = null)
        {
            var found = false;
            foreach (XrefInstance xref in XrefScanner.XrefScan(methodBase))
            {
                if (xref.Type != XrefType.Method) return false;

                MethodBase resolved = xref.TryResolve();
                if (resolved == null) return false;

                if (!string.IsNullOrEmpty(methodName))
                    found = !string.IsNullOrEmpty(resolved.Name) && resolved.Name.IndexOf(methodName, StringComparison.OrdinalIgnoreCase) >= 0;

                if (!string.IsNullOrEmpty(reflectedType))
                    found = !string.IsNullOrEmpty(resolved.ReflectedType?.Name) && resolved.ReflectedType.Name.IndexOf(
                                reflectedType,
                                StringComparison.OrdinalIgnoreCase) >= 0;

                if (found) return true;
            }

            return false;
        }

        internal delegate void AlignTrackingToPlayerDelegate();

    }

}