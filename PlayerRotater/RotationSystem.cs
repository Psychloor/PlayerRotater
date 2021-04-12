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

        internal static RotateAroundEnum RotateAround = RotateAroundEnum.Hips;

        internal static bool InvertPitch;

        private Utilities.AlignTrackingToPlayerDelegate alignTrackingToPlayer;

        private Transform cameraTransform;

        private Vector3 originalGravity;

        private Transform playerTransform, originTransform;

        private bool rotating;

        private bool usePlayerAxis;

        internal bool WorldAllowed;

        private RotationSystem()
        { }

        // For emmVRC and other mods to be able to check for
        // needs to fly so other mods can break it/this could break them
        public static bool Rotating => Instance.rotating;

        internal void Pitch(float amount)
        {
            if (InvertPitch) amount *= -1;
            playerTransform.RotateAround(
                originTransform.position,
                usePlayerAxis ? playerTransform.right : originTransform.right,
                amount * RotationSpeed * Time.deltaTime);
        }

        internal void Yaw(float amount)
        {
            playerTransform.RotateAround(
                originTransform.position,
                usePlayerAxis ? playerTransform.up : originTransform.up,
                amount * RotationSpeed * Time.deltaTime);
        }

        internal void Roll(float amount)
        {
            playerTransform.RotateAround(
                originTransform.position,
                usePlayerAxis ? playerTransform.forward : originTransform.forward,
                -amount * RotationSpeed * Time.deltaTime);
        }

        internal static bool Initialize()
        {
            if (Instance != null) return false;
            Instance = new RotationSystem();
            MelonCoroutines.Start(GrabMainCamera());
            return true;
        }

        private static IEnumerator GrabMainCamera()
        {
            while (!Instance.cameraTransform)
            {
                yield return new WaitForSeconds(1f);
                foreach (Object component in Object.FindObjectsOfType(Il2CppType.Of<Transform>()))
                {
                    yield return null;
                    Transform transform;
                    if ((transform = component.TryCast<Transform>()) == null) continue;
                    if (!transform.name.Equals("Camera (eye)", StringComparison.OrdinalIgnoreCase)) continue;
                    Instance.cameraTransform = transform;
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

        private void GrabOriginTransform()
        {
            var isHumanoid = false;
            switch (RotateAround)
            {
                case RotateAroundEnum.Hips:
                    // ReSharper disable twice Unity.NoNullPropagation
                    GameObject localAvatar = Utilities.GetLocalVRCPlayer()?.prop_VRCAvatarManager_0?.prop_GameObject_0;
                    Animator localAnimator = localAvatar?.GetComponent<Animator>();

                    if (localAnimator != null)
                    {
                        isHumanoid = localAnimator.isHuman;
                        originTransform = isHumanoid ? localAnimator.GetBoneTransform(HumanBodyBones.Hips) : cameraTransform;
                    }
                    else
                    {
                        originTransform = cameraTransform;
                    }

                    break;

                case RotateAroundEnum.ViewPoint:
                    originTransform = cameraTransform;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(RotateAround), RotateAround, "What kind of dinkleberry thing did you do to my enum?");
            }

            usePlayerAxis = RotateAround == RotateAroundEnum.Hips && isHumanoid;
        }

        internal void ToggleNoClip()
        {
            if (!playerTransform) return;
            CharacterController characterController = playerTransform.GetComponent<CharacterController>();
            if (!characterController) return;

            if (rotating) GrabOriginTransform();

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

            if (CurrentControlScheme.HandleInput(playerTransform, cameraTransform, FlyingSpeed))
                alignTrackingToPlayer();
        }

        internal void OnLeftWorld()
        {
            WorldAllowed = false;
            rotating = false;
            playerTransform = null;
            alignTrackingToPlayer = null;
        }

        internal enum RotateAroundEnum
        {

            Hips,

            ViewPoint

        }

        private IEnumerator BarrelRollCoroutine()
        {
            bool originalRotated = rotating;
            if (!originalRotated) Toggle();

            var degreesCompleted = 0f;
            while (degreesCompleted < 360f)
            {
                yield return new WaitForEndOfFrame();
                float currentRoll = 360f * Time.deltaTime;

                playerTransform.RotateAround(originTransform.position, usePlayerAxis ? playerTransform.forward : originTransform.forward, -currentRoll);

                degreesCompleted += currentRoll;
            }

            if (!originalRotated) Toggle();
        }

        public void BarrelRoll()
        {
            if (WorldAllowed)
                MelonCoroutines.Start(BarrelRollCoroutine());
        }

    }

}