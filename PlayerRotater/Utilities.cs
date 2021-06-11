namespace PlayerRotater
{

    using System;
    using System.Collections;
    using System.Linq;
    using System.Reflection;

    using Il2CppSystem.Collections.Generic;

    using MelonLoader;

    using UnhollowerRuntimeLib.XrefScans;

    using UnityEngine;

    using VRC.Core;

    internal static class Utilities
    {

        public delegate bool StreamerModeDelegate();

        private static MethodInfo alignTrackingToPlayerMethod;

        private static StreamerModeDelegate ourStreamerModeDelegate;

        // Yes that's a lot of xref scanning but gotta make sure xD
        // Only grabs once anyway ¯\_(ツ)_/¯
        internal static AlignTrackingToPlayerDelegate GetAlignTrackingToPlayerDelegate
        {
            get
            {
                alignTrackingToPlayerMethod ??= typeof(VRCPlayer).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).First(
                    m => m.ReturnType == typeof(void)
                         && m.GetParameters().Length == 0
                         && m.Name.IndexOf("PDM", StringComparison.OrdinalIgnoreCase) == -1
                         && m.XRefScanForMethod("get_Transform")

                         //&& m.XRefScanForMethod(reflectedType: "Player")
                         //&& m.XRefScanForMethod("Vector3_Quaternion")
                         && m.XRefScanForMethod(reflectedType: nameof(VRCTrackingManager))
                         && m.XRefScanForMethod(reflectedType: nameof(InputStateController)));

                return (AlignTrackingToPlayerDelegate)Delegate.CreateDelegate(
                    typeof(AlignTrackingToPlayerDelegate),
                    GetLocalVRCPlayer(),
                    alignTrackingToPlayerMethod);
            }
        }

        public static StreamerModeDelegate GetStreamerMode
        {
            get
            {
                if (ourStreamerModeDelegate != null) return ourStreamerModeDelegate;

                foreach (PropertyInfo property in typeof(VRCInputManager).GetProperties(BindingFlags.Public | BindingFlags.Static))
                {
                    if (property.PropertyType != typeof(bool)) continue;
                    if (XrefScanner.XrefScan(property.GetSetMethod()).Any(
                        xref => xref.Type == XrefType.Global && xref.ReadAsObject()?.ToString().Equals("VRC_STREAMER_MODE_ENABLED") == true))
                    {
                        ourStreamerModeDelegate = (StreamerModeDelegate)Delegate.CreateDelegate(typeof(StreamerModeDelegate), property.GetGetMethod());
                        return ourStreamerModeDelegate;
                    }
                }

                return null;
            }
        }

        internal static GameObject lockRotationButton, toggleRotaterButton;

        private static void SetRotationButtons(bool enabled)
        {
            lockRotationButton.SetActive(enabled);
            toggleRotaterButton.SetActive(enabled);
        }
        

        internal static bool IsInVR { get; set; }

        internal static void LogDebug(string text)
        {
        #if DEBUG
            MelonLogger.Msg(ConsoleColor.DarkGreen, text);
        #endif
        }

        // Borrowed from https://github.com/gompocp/ActionMenuUtils/blob/69f1fe1852810ee977f23dceee5cff0e7b4528d7/ActionMenuAPI.cs#L251
        internal static bool AnyActionMenuesOpen()
        {
            return ActionMenuDriver.field_Public_Static_ActionMenuDriver_0.field_Public_ActionMenuOpener_0.field_Private_Boolean_0
                   || ActionMenuDriver.field_Public_Static_ActionMenuDriver_0.field_Public_ActionMenuOpener_1.field_Private_Boolean_0;
        }

        internal static IEnumerator CheckWorld()
        {
            LogDebug("Checking World");
            string worldId = RoomManager.field_Internal_Static_ApiWorld_0.id;
            RotationSystem.Instance.WorldAllowed = false;
            SetRotationButtons(false);

            // Check if black/whitelisted from EmmVRC - thanks Emilia and the rest of EmmVRC Staff
            WWW www = new($"https://dl.emmvrc.com/riskyfuncs.php?worldid={worldId}", null, new Dictionary<string, string>());
            while (!www.isDone)
                yield return new WaitForEndOfFrame();
            string result = www.text?.Trim().ToLower();
            www.Dispose();
            if (!string.IsNullOrWhiteSpace(result))
                switch (result)
                {
                    case "allowed":
                        RotationSystem.Instance.WorldAllowed = true;
                        SetRotationButtons(true);
                        LogDebug("EmmVRC Allowed");
                        yield break;

                    case "denied":
                        RotationSystem.Instance.WorldAllowed = false;
                        SetRotationButtons(false);
                        LogDebug("EmmVRC Disallowed");
                        yield break;
                }

            LogDebug("Checking World Tags, no response from EmmVRC");

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
                                    if (worldTag.IndexOf("game", StringComparison.OrdinalIgnoreCase) != -1
                                        || worldTag.IndexOf("club", StringComparison.OrdinalIgnoreCase) != -1)
                                    {
                                        LogDebug("Found Game/Club Tag(s)");
                                        RotationSystem.Instance.WorldAllowed = false;
                                        SetRotationButtons(false);
                                        return;
                                    }

                                RotationSystem.Instance.WorldAllowed = true;
                                SetRotationButtons(true);
                            }
                            else
                            {
                                MelonLogger.Error("Failed to cast ApiModel to ApiWorld");
                            }
                        }),
                disableCache: false);
        }

        internal static VRCPlayer GetLocalVRCPlayer()
        {
            return VRCPlayer.field_Internal_Static_VRCPlayer_0;
        }

        internal static bool XRefScanForMethod(this MethodBase methodBase, string methodName = null, string reflectedType = null)
        {
            var found = false;
            foreach (XrefInstance xref in XrefScanner.XrefScan(methodBase))
            {
                if (xref.Type != XrefType.Method) continue;

                MethodBase resolved = xref.TryResolve();
                if (resolved == null) continue;

                if (!string.IsNullOrEmpty(methodName))
                    found = !string.IsNullOrEmpty(resolved.Name) && resolved.Name.IndexOf(methodName, StringComparison.OrdinalIgnoreCase) >= 0;

                if (!string.IsNullOrEmpty(reflectedType))
                    found = !string.IsNullOrEmpty(resolved.ReflectedType?.Name)
                            && resolved.ReflectedType.Name.IndexOf(reflectedType, StringComparison.OrdinalIgnoreCase) >= 0;

                if (found) return true;
            }

            return false;
        }

        internal delegate void AlignTrackingToPlayerDelegate();

    }

}