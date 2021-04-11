namespace PlayerRotater
{

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MelonLoader;

    using PlayerRotater.ControlSchemes;

    using UIExpansionKit.API;

    public class ModMain : MelonMod
    {

        private const string SettingsCategory = "PlayerRotater";

        /// <summary>
        ///     Russian National Anthem Plays
        /// </summary>
        private static MelonPreferences_Category ourCategory;

        private List<(string SettingsValue, string DisplayName)> controlSchemes;

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

            controlSchemes = new List<(string SettingsValue, string DisplayName)> { ("default", "Default"), ("jannyaa", "JanNyaa's") };

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

            ourCategory = MelonPreferences.CreateCategory(SettingsCategory, "Player Rotater");
            ourCategory.CreateEntry("NoClip", RotationSystem.NoClipFlying, "No-Clipping (Desktop)");
            ourCategory.CreateEntry("RotationSpeed", RotationSystem.RotationSpeed, "Rotation Speed");
            ourCategory.CreateEntry("FlyingSpeed", RotationSystem.FlyingSpeed, "Flying Speed");
            ourCategory.CreateEntry("ControlScheme", RotationSystem.CurrentControlSchemeName, "Control Scheme");
            ExpansionKitApi.RegisterSettingAsStringEnum(SettingsCategory, "ControlScheme", controlSchemes);

            LoadSettings();
        }

        private static void LoadSettings()
        {
            try
            {
                RotationSystem.NoClipFlying = ourCategory.GetEntry<bool>("NoClip").Value;
                RotationSystem.RotationSpeed = ourCategory.GetEntry<float>("RotationSpeed").Value;
                RotationSystem.FlyingSpeed = ourCategory.GetEntry<float>("FlyingSpeed").Value;

                RotationSystem.CurrentControlSchemeName = ourCategory.GetEntry<string>("ControlScheme").Value;
                switch (RotationSystem.CurrentControlSchemeName)
                {
                    case "default":
                        RotationSystem.CurrentControlScheme = new DefaultControlScheme();
                        break;

                    case "jannyaa":
                        RotationSystem.CurrentControlScheme = new JanNyaaControlScheme();
                        break;
                }

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