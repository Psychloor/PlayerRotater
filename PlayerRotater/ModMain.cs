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
        ///     https://www.youtube.com/watch?v=U06jlgpMtQs
        /// </summary>
        private static MelonPreferences_Category ourCategory;

        private List<(string SettingsValue, string DisplayName)> controlSchemes;

        private bool failedToLoad;

        private List<(string SettingsValue, string DisplayName)> rotationOrigins;

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
            rotationOrigins = new List<(string SettingsValue, string DisplayName)> { ("hips", "Hips (Generic Viewpoint)"), ("viewpoint", "View Point") };

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
            ourCategory.CreateEntry("InvertPitch", RotationSystem.InvertPitch, "Invert Pitch");

            ourCategory.CreateEntry("ControlScheme", "default", "Control Scheme");
            ExpansionKitApi.RegisterSettingAsStringEnum(SettingsCategory, "ControlScheme", controlSchemes);

            ourCategory.CreateEntry("RotationOrigin", "hips", "Rotation Origin");
            ExpansionKitApi.RegisterSettingAsStringEnum(SettingsCategory, "RotationOrigin", rotationOrigins);

            LoadSettings();
        }

        private static void LoadSettings()
        {
            try
            {
                RotationSystem.NoClipFlying = ourCategory.GetEntry<bool>("NoClip").Value;
                RotationSystem.RotationSpeed = ourCategory.GetEntry<float>("RotationSpeed").Value;
                RotationSystem.FlyingSpeed = ourCategory.GetEntry<float>("FlyingSpeed").Value;
                RotationSystem.InvertPitch = ourCategory.GetEntry<bool>("InvertPitch").Value;

                switch (ourCategory.GetEntry<string>("ControlScheme").Value)
                {
                    default:
                        ourCategory.GetEntry<string>("ControlScheme").Value = "default";
                        ourCategory.GetEntry<string>("ControlScheme").Save();

                        RotationSystem.CurrentControlScheme = new DefaultControlScheme();
                        break;

                    case "default":
                        RotationSystem.CurrentControlScheme = new DefaultControlScheme();
                        break;

                    case "jannyaa":
                        RotationSystem.CurrentControlScheme = new JanNyaaControlScheme();
                        break;
                }

                switch (ourCategory.GetEntry<string>("RotationOrigin").Value)
                {
                    default:
                        ourCategory.GetEntry<string>("RotationOrigin").Value = "hips";
                        ourCategory.GetEntry<string>("RotationOrigin").Save();

                        RotationSystem.RotateAround = RotationSystem.RotateAroundEnum.Hips;
                        break;

                    case "hips":
                        RotationSystem.RotateAround = RotationSystem.RotateAroundEnum.Hips;
                        break;

                    case "viewpoint":
                        RotationSystem.RotateAround = RotationSystem.RotateAroundEnum.ViewPoint;
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
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Toggle\nPlayer\nRotation", () => RotationSystem.Instance.Toggle());
            //ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Do A\nBarrel Roll", () => RotationSystem.Instance.BarrelRoll());
        }

        public override void OnUpdate()
        {
            if (failedToLoad) return;
            RotationSystem.Instance.OnUpdate();
        }

    }

}