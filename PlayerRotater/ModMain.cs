namespace PlayerRotater
{

    using System;
    using System.Collections;

    using MelonLoader;

    using UIExpansionKit.API;

    using UnityEngine;
    using UnityEngine.XR;

    public class ModMain : MelonMod
    {

        private const string SettingsCategory = "PlayerRotater";

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

            SetupSettings();
        }

        public override void OnModSettingsApplied()
        {
            if (failedToLoad) return;
            LoadSettings();
        }

        private void SetupSettings()
        {
            if (failedToLoad) return;
            MelonPrefs.RegisterCategory(SettingsCategory, "Player Rotater");
            MelonPrefs.RegisterBool(SettingsCategory, "NoClip", RotationSystem.NoClipFlying, "No-Clipping");
            MelonPrefs.RegisterFloat(SettingsCategory, "RotationSpeed", RotationSystem.RotationSpeed, "Rotation Speed");
            MelonPrefs.RegisterFloat(SettingsCategory, "FlyingSpeed", RotationSystem.FlyingSpeed, "Flying Speed");
            LoadSettings();
        }

        private static void LoadSettings()
        {
            try
            {
                RotationSystem.NoClipFlying = MelonPrefs.GetBool(SettingsCategory, "NoClip");
                RotationSystem.RotationSpeed = MelonPrefs.GetFloat(SettingsCategory, "RotationSpeed");
                RotationSystem.FlyingSpeed = MelonPrefs.GetFloat(SettingsCategory, "FlyingSpeed");

                RotationSystem.Instance.ToggleNoClip();
            }
            catch (Exception e)
            {
                MelonLogger.LogError("Failed to Load Settings: " + e);
            }
        }

        private static IEnumerator SetupUI()
        {
            while (QuickMenu.prop_QuickMenu_0 == null) yield return new WaitForSeconds(1f);
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Toggle\nRotation\nMode", () => RotationSystem.Instance.Toggle());
        }

        public override void OnUpdate()
        {
            if (failedToLoad) return;
            RotationSystem.Instance.OnUpdate();
        }

    }

}