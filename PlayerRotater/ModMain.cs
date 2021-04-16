namespace PlayerRotater
{

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MelonLoader;

    using PlayerRotater.ControlSchemes;

    using UIExpansionKit.API;

    using UnityEngine;

    public class ModMain : MelonMod
    {

        private const string SettingsIdentifier = "PlayerRotater";

        /// <summary>
        ///     https://www.youtube.com/watch?v=U06jlgpMtQs
        /// </summary>
        private static MelonPreferences_Category ourCategory;

        private static MelonPreferences_Entry<bool> noClippingEntry, invertPitchEntry;

        private static MelonPreferences_Entry<float> flyingSpeedEntry, rotationSpeedEntry;

        private static MelonPreferences_Entry<string> controlSchemeEntry, rotationOriginEntry;

        private List<(string SettingsValue, string DisplayName)> controlSchemes;

        private bool failedToLoad;

        private static bool easterEgg;

        private List<(string SettingsValue, string DisplayName)> rotationOrigins;

        public override void OnApplicationStart()
        {
            Utilities.IsInVR = Environment.GetCommandLineArgs().All(args => !args.Equals("--no-vr", StringComparison.OrdinalIgnoreCase));
            easterEgg = Environment.GetCommandLineArgs().Any(arg => arg.IndexOf("barrelroll", StringComparison.OrdinalIgnoreCase) != -1);
            
            if (!RotationSystem.Initialize())
            {
                MelonLogger.Msg("Failed to initialize the rotation system. Instance already exists");
                failedToLoad = true;
                return;
            }

            controlSchemes = new List<(string SettingsValue, string DisplayName)> { ("default", "Default"), ("jannyaa", "JanNyaa's") };
            rotationOrigins = new List<(string SettingsValue, string DisplayName)>
                                  {
                                      ("hips", "Hips"), ("viewpoint", "View Point/Camera"), ("righthand", "Right Hand"), ("lefthand", "Left Hand")
                                  };

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

            ourCategory = MelonPreferences.CreateCategory(SettingsIdentifier, BuildInfo.Name);
            noClippingEntry = ourCategory.CreateEntry("NoClip", RotationSystem.NoClipFlying, "No-Clipping (Desktop)") as MelonPreferences_Entry<bool>;
            rotationSpeedEntry = ourCategory.CreateEntry("RotationSpeed", RotationSystem.RotationSpeed, "Rotation Speed") as MelonPreferences_Entry<float>;
            flyingSpeedEntry = ourCategory.CreateEntry("FlyingSpeed", RotationSystem.FlyingSpeed, "Flying Speed") as MelonPreferences_Entry<float>;
            invertPitchEntry = ourCategory.CreateEntry("InvertPitch", RotationSystem.InvertPitch, "Invert Pitch") as MelonPreferences_Entry<bool>;

            controlSchemeEntry = ourCategory.CreateEntry("ControlScheme", "default", "Control Scheme") as MelonPreferences_Entry<string>;
            ExpansionKitApi.RegisterSettingAsStringEnum(ourCategory.Identifier, controlSchemeEntry?.Identifier, controlSchemes);

            rotationOriginEntry = ourCategory.CreateEntry("RotationOrigin", "hips", "Humanoid Rotation Origin") as MelonPreferences_Entry<string>;
            ExpansionKitApi.RegisterSettingAsStringEnum(ourCategory.Identifier, rotationOriginEntry?.Identifier, rotationOrigins);

            LoadSettings();
        }

        private static void LoadSettings()
        {
            try
            {
                RotationSystem.NoClipFlying = noClippingEntry.Value;
                RotationSystem.RotationSpeed = rotationSpeedEntry.Value;
                RotationSystem.FlyingSpeed = flyingSpeedEntry.Value;
                RotationSystem.InvertPitch = invertPitchEntry.Value;

                switch (controlSchemeEntry.Value)
                {
                    default:
                        controlSchemeEntry.ResetToDefault();
                        controlSchemeEntry.Save();

                        RotationSystem.CurrentControlScheme = new DefaultControlScheme();
                        break;

                    case "default":
                        RotationSystem.CurrentControlScheme = new DefaultControlScheme();
                        break;

                    case "jannyaa":
                        RotationSystem.CurrentControlScheme = new JanNyaaControlScheme();
                        break;
                }

                switch (rotationOriginEntry.Value)
                {
                    default:
                        rotationOriginEntry.ResetToDefault();
                        rotationOriginEntry.Save();

                        RotationSystem.RotationOrigin = RotationSystem.RotationOriginEnum.Hips;
                        break;

                    case "hips":
                        RotationSystem.RotationOrigin = RotationSystem.RotationOriginEnum.Hips;
                        break;

                    case "viewpoint":
                        RotationSystem.RotationOrigin = RotationSystem.RotationOriginEnum.ViewPoint;
                        break;

                    // ReSharper disable once StringLiteralTypo
                    case "righthand":
                        RotationSystem.RotationOrigin = RotationSystem.RotationOriginEnum.RightHand;
                        break;

                    // ReSharper disable once StringLiteralTypo
                    case "lefthand":
                        RotationSystem.RotationOrigin = RotationSystem.RotationOriginEnum.LeftHand;
                        break;
                }

                RotationSystem.Instance.UpdateSettings();
            }
            catch (Exception e)
            {
                MelonLogger.Msg("Failed to Load Settings: " + e);
            }
        }

        private static void SetupUI()
        {
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Toggle\nPlayer\nRotation", () => RotationSystem.Instance.Toggle());

            // shhhhhhh (✿❦ ͜ʖ ❦)
            if (easterEgg)
                ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Do A\nBarrel Roll", () => RotationSystem.Instance.BarrelRoll());
        }
        

        public override void OnUpdate()
        {
            if (!easterEgg) return;
            if (RotationSystem.BarrelRolling) return;
            if (!Input.GetKeyDown(KeyCode.B)) return;
            
            if (Input.GetKey(KeyCode.LeftShift)
                && Input.GetKey(KeyCode.LeftControl))
            {
                RotationSystem.Instance.BarrelRoll();
            }
        }

        public override void OnFixedUpdate()
        {
            if (failedToLoad) return;
            RotationSystem.Instance.Update();
        }

    }

}