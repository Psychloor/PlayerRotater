namespace PlayerRotater
{

    using System;
    using System.Collections;

    using MelonLoader;

    using PlayerRotater.ControlSchemes.Interface;

    using UnhollowerRuntimeLib;

    using UnityEngine;

    using Object = UnityEngine.Object;

    public class RotationSystem
    {

        internal static float FlyingSpeed = 5f;

        internal static float RotationSpeed = 180f;

        internal static bool NoClipFlying = true;

        internal static RotationSystem Instance;

        internal static IControlScheme CurrentControlScheme;

        internal static string CurrentControlSchemeName = "default";

        private Utilities.AlignTrackingToPlayerDelegate alignTrackingToPlayer;

        public Transform CameraTransform;

        private Vector3 originalGravity;

        private Transform playerTransform;

        private bool rotating;

        internal bool WorldAllowed;

        private RotationSystem()
        { }

        // For emmVRC and other mods to be able to check for
        // needs to fly so other mods can break it/this could break them
        public static bool Rotating => Instance.rotating;

        internal static bool Initialize()
        {
            if (Instance != null) return false;
            Instance = new RotationSystem();
            MelonCoroutines.Start(GrabMainCamera());
            return true;
        }

        private static IEnumerator GrabMainCamera()
        {
            while (!Instance.CameraTransform)
            {
                yield return new WaitForSeconds(1f);
                foreach (Object component in Object.FindObjectsOfType(Il2CppType.Of<Transform>()))
                {
                    yield return null;
                    Transform transform;
                    if ((transform = component.TryCast<Transform>()) == null) continue;
                    if (!transform.name.Equals("Camera (eye)", StringComparison.OrdinalIgnoreCase)) continue;
                    Instance.CameraTransform = transform;
                    break;
                }
            }
        }

        // bit weird but i've gotten some errors few times where it bugged out a bit
        internal void Toggle()
        {
            Utilities.LogDebug("Toggling, current state: " + rotating);
            if (!WorldAllowed) return;
            if (!rotating) originalGravity = Physics.gravity;

            try
            {
                playerTransform ??= Utilities.GetLocalVRCPlayer().transform;
                rotating = !rotating;

                if (rotating)
                {
                    originalGravity = Physics.gravity;
                    Physics.gravity = Vector3.zero;
                    alignTrackingToPlayer ??= Utilities.GetAlignTrackingToPlayerDelegate;
                }
                else
                {
                    Quaternion local = playerTransform.localRotation;
                    playerTransform.localRotation = new Quaternion(0f, local.y, 0f, local.w);
                    Physics.gravity = originalGravity;
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error("Error Toggling: " + e);
                rotating = false;
            }

            ToggleNoClip();

            Utilities.LogDebug("Toggling end, new state: " + rotating);

            if (rotating) return;
            Physics.gravity = originalGravity;
            alignTrackingToPlayer?.Invoke();
        }

        internal void ToggleNoClip()
        {
            if (!playerTransform) return;
            CharacterController characterController = playerTransform.GetComponent<CharacterController>();
            if (!characterController) return;

            if (rotating && !Utilities.IsVR) characterController.enabled = !NoClipFlying;
            else if (!characterController.enabled)
                characterController.enabled = true;

            if (Utilities.IsVR)
                Utilities.GetLocalVRCPlayer()?.prop_VRCPlayerApi_0.Immobilize(rotating);
        }

        internal void OnUpdate()
        {
            if (!rotating
                || !WorldAllowed) return;

            if (CurrentControlScheme.HandleInput(playerTransform, CameraTransform, FlyingSpeed, RotationSpeed))
                alignTrackingToPlayer();
        }

        internal void OnLeftWorld()
        {
            WorldAllowed = false;
            rotating = false;
            playerTransform = null;
            alignTrackingToPlayer = null;
        }

    }

}