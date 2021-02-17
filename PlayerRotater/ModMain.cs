namespace PlayerRotater
{

    using System;
    using System.Collections;
    using System.Linq;

    using MelonLoader;

    using UIExpansionKit.API;

    using UnityEngine;

    public class ModMain : MelonMod
    {

        private const string SettingsCategory = "PlayerRotater";

        /// <summary>
        /// Russian National Anthem Plays
        /// </summary>
        private static MelonPreferences_Category OurCategory;

        private bool failedToLoad;

        public override void OnApplicationStart()
        {
            Utilities.IsVR = !Environment.GetCommandLineArgs().Any(args => args.Equals("--no-vr", StringComparison.OrdinalIgnoreCase));
            if (!RotationSystem.Initialize())
            {
                MelonLogger.Msg("Failed to initialize the rotation system. Instance already exists");
                failedToLoad = true;
                return;
            }

            ModPatches.Patch(Harmony);
            SetupUI();

            SetupSettings();
        }

        public override void OnPreferencesSaved()
        {
            if (failedToLoad) return;
            LoadSettings();
        }

        private void SetupSettings()
        {
            if (failedToLoad) return;

            OurCategory = MelonPreferences.CreateCategory(SettingsCategory, "Player Rotater");
            OurCategory.CreateEntry("NoClip", RotationSystem.NoClipFlying, "No-Clipping (Desktop)");
            OurCategory.CreateEntry("RotationSpeed", RotationSystem.RotationSpeed, "Rotation Speed");
            OurCategory.CreateEntry("FlyingSpeed", RotationSystem.FlyingSpeed, "Flying Speed");

            LoadSettings();
        }

        private static void LoadSettings()
        {
            try
            {
                RotationSystem.NoClipFlying = OurCategory.GetEntry<bool>("NoClip").Value;
                RotationSystem.RotationSpeed = OurCategory.GetEntry<float>("RotationSpeed").Value;
                RotationSystem.FlyingSpeed = OurCategory.GetEntry<float>("FlyingSpeed").Value;

                RotationSystem.Instance.ToggleNoClip();
            }
            catch (Exception e)
            {
                MelonLogger.Msg("Failed to Load Settings: " + e);
            }
        }

        private static void SetupUI()
        {
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Toggle\nRotation\nMode", () => RotationSystem.Instance.Toggle());
        }

        public override void OnUpdate()
        {
            if (failedToLoad) return;
            RotationSystem.Instance.OnUpdate();
        }

    }

}