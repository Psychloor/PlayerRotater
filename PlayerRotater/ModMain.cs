namespace PlayerRotater
{

    using System.Collections;

    using MelonLoader;

    using UIExpansionKit.API;

    using UnityEngine;

    public class ModMain : MelonMod
    {

        private bool failedToLoad;

        public override void OnApplicationStart()
        {
            if (!RotationSystem.Initialize())
            {
                MelonLogger.LogError("Failed to initialize the rotation system. Instance already exists");
                failedToLoad = true;
                return;
            }

            ModPatches.Patch(harmonyInstance);
            ExpansionKitApi.RegisterWaitConditionBeforeDecorating(SetupUI());
        }

        private IEnumerator SetupUI()
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