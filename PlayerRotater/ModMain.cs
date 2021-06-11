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

        private static bool easterEgg;

        private List<(string SettingsValue, string DisplayName)> controlSchemes, rotationOrigins;

        private bool failedToLoad;

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

            if (!ModPatches.PatchMethods())
            {
                failedToLoad = true;
                MelonLogger.Warning("Failed to patch everything, disabling player rotater");
                return;
            }

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

            controlSchemes = new List<(string SettingsValue, string DisplayName)> { ("default", "Default"), ("jannyaa", "JanNyaa's") };
            rotationOrigins = new List<(string SettingsValue, string DisplayName)>
                                  {
                                      ("hips", "Hips"), ("viewpoint", "View Point/Camera"), ("righthand", "Right Hand"), ("lefthand", "Left Hand")
                                  };

            ourCategory = MelonPreferences.CreateCategory(SettingsIdentifier, BuildInfo.Name);
            noClippingEntry = ourCategory.CreateEntry("NoClip", RotationSystem.NoClipFlying, "No-Clipping (Desktop)");
            rotationSpeedEntry = ourCategory.CreateEntry("RotationSpeed", RotationSystem.RotationSpeed, "Rotation Speed");
            flyingSpeedEntry = ourCategory.CreateEntry("FlyingSpeed", RotationSystem.FlyingSpeed, "Flying Speed");
            invertPitchEntry = ourCategory.CreateEntry("InvertPitch", RotationSystem.InvertPitch, "Invert Pitch");

            controlSchemeEntry = ourCategory.CreateEntry("ControlScheme", "default", "Control Scheme");
            ExpansionKitApi.RegisterSettingAsStringEnum(ourCategory.Identifier, controlSchemeEntry?.Identifier, controlSchemes);

            rotationOriginEntry = ourCategory.CreateEntry("RotationOrigin", "hips", "Humanoid Rotation Origin");
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
            ICustomLayoutedMenu quickMenu = ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu);
            quickMenu.AddToggleButton("Player\nRotater", b => RotationSystem.Instance.Toggle(), () => RotationSystem.Rotating, o => Utilities.toggleRotaterButton = o);
            quickMenu.AddToggleButton("Rotater\nLock\nRotation", b => RotationSystem.LockRotation = b, () => RotationSystem.LockRotation, o => Utilities.lockRotationButton = o);

            // shhhhhhh (✿❦ ͜ʖ ❦)
            if (easterEgg)
                quickMenu.AddSimpleButton("Do A\nBarrel Roll", () => RotationSystem.Instance.BarrelRoll());
        }

        public override void OnUpdate()
        {
            if (failedToLoad) return;
            RotationSystem.Instance.Update();
            if (!easterEgg) return;
            if (RotationSystem.BarrelRolling) return;
            if (!Input.GetKeyDown(KeyCode.B)) return;

            if (Input.GetKey(KeyCode.LeftShift)
                && Input.GetKey(KeyCode.LeftControl)) RotationSystem.Instance.BarrelRoll();
        }

    }

}