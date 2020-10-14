namespace PlayerRotater
{

    using System.Collections;

    using MelonLoader;

    using UIExpansionKit.API;

    using UnityEngine;
    using UnityEngine.XR;

    public class ModMain : MelonMod
    {

        private bool failedToLoad;

        public override void OnApplicationStart()
        {
            if (XRDevice.isPresent)
            {
                MelonLogger.LogWarning("VR Headset Detected. Rotation in VR doesn't work and turns into Spinning Mod\nDisabling this mod");
                failedToLoad = true;
                return;
            }

            if (!RotationSystem.Initialize())
            {
                MelonLogger.LogError("Failed to initialize the rotation system. Instance already exists");
                failedToLoad = true;
                return;
            }

            ModPatches.Patch(harmonyInstance);
            ExpansionKitApi.RegisterWaitConditionBeforeDecorating(SetupUI());
        }

        private static IEnumerator SetupUI()
        {
            while (QuickMenu.prop_QuickMenu_0 == null) yield return new WaitForSeconds(1f);
            ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.QuickMenu, "Toggle\nRotation\nMode", () => RotationSystem.Instance.Toggle());
        }

        public override void OnUpdate()
        {
            if (failedToLoad) return;
            RotationSystem.Instance.OnUpdate();
        }

    }

}