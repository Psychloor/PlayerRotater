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

        internal static RotationOriginEnum RotationOrigin = RotationOriginEnum.Hips;

        internal static bool InvertPitch, BarrelRolling, LockRotation;

        private Utilities.AlignTrackingToPlayerDelegate alignTrackingToPlayer;

        private Transform cameraTransform;

        private Vector3 originalGravity;

        private Transform playerTransform, originTransform;

        private bool rotating;

        private bool usePlayerAxis, holdingShift;

        internal bool WorldAllowed;

        private RotationSystem()
        { }

        public static bool IsWorldAllowed => Instance.WorldAllowed;

        // For emmVRC and other mods to be able to check for
        // needs to fly so other mods can break it/this could break them
        public static bool Rotating => Instance.rotating;

        internal void Pitch(float inputAmount)
        {
            if (InvertPitch) inputAmount *= -1;
            playerTransform.RotateAround(
                originTransform.position,
                usePlayerAxis ? playerTransform.right : originTransform.right,
                inputAmount * RotationSpeed * Time.deltaTime * (holdingShift ? 2f : 1f));
        }

        internal void Yaw(float inputAmount)
        {
            playerTransform.RotateAround(
                originTransform.position,
                usePlayerAxis ? playerTransform.up : originTransform.up,
                inputAmount * RotationSpeed * Time.deltaTime * (holdingShift ? 2f : 1f));
        }

        internal void Roll(float inputAmount)
        {
            playerTransform.RotateAround(
                originTransform.position,
                usePlayerAxis ? -playerTransform.forward : -originTransform.forward,
                inputAmount * RotationSpeed * Time.deltaTime * (holdingShift ? 2f : 1f));
        }

        internal void Fly(float inputAmount, Vector3 direction)
        {
            playerTransform.position += direction * inputAmount * FlyingSpeed * (holdingShift ? 2f : 1f) * Time.deltaTime;
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

            if (Utilities.GetStreamerMode) rotating = false;

            UpdateSettings();

            Utilities.LogDebug("Toggling end, new state: " + rotating);

            if (rotating) return;
            Physics.gravity = originalGravity;
            alignTrackingToPlayer?.Invoke();
        }

        private void GrabOriginTransform()
        {
            var isHumanoid = false;

            void GetHumanBoneTransform(HumanBodyBones bone)
            {
                // ReSharper disable twice Unity.NoNullPropagation
                GameObject localAvatar = Utilities.GetLocalVRCPlayer()?.prop_VRCAvatarManager_0?.prop_GameObject_0;
                Animator localAnimator = localAvatar?.GetComponent<Animator>();

                if (localAnimator != null)
                {
                    isHumanoid = localAnimator.isHuman;
                    originTransform = isHumanoid ? localAnimator.GetBoneTransform(bone) : cameraTransform;
                }
                else
                {
                    originTransform = cameraTransform;
                }
            }

            switch (RotationOrigin)
            {
                case RotationOriginEnum.Hips:
                    GetHumanBoneTransform(HumanBodyBones.Hips);
                    break;

                case RotationOriginEnum.ViewPoint:
                    originTransform = cameraTransform;
                    break;

                case RotationOriginEnum.RightHand:
                    GetHumanBoneTransform(HumanBodyBones.RightHand);
                    break;

                case RotationOriginEnum.LeftHand:
                    GetHumanBoneTransform(HumanBodyBones.LeftHand);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(RotationOrigin), RotationOrigin, "What kind of dinkleberry thing did you do to my enum?");
            }

            usePlayerAxis = RotationOrigin == RotationOriginEnum.Hips && isHumanoid;
        }

        internal void UpdateSettings()
        {
            if (!playerTransform) return;
            CharacterController characterController = playerTransform.GetComponent<CharacterController>();
            if (!characterController) return;

            if (rotating) GrabOriginTransform();

            if (rotating && !Utilities.IsInVR) characterController.enabled = !NoClipFlying;
            else if (!characterController.enabled)
                characterController.enabled = true;

            if (Utilities.IsInVR)
                Utilities.GetLocalVRCPlayer()?.prop_VRCPlayerApi_0.Immobilize(rotating);
        }

        internal void Update()
        {
            if (!rotating
                || !WorldAllowed) return;

            holdingShift = Input.GetKey(KeyCode.LeftShift);
            if (!BarrelRolling
                && CurrentControlScheme.HandleInput(playerTransform, cameraTransform))
                alignTrackingToPlayer();
        }

        internal void OnLeftWorld()
        {
            WorldAllowed = false;
            rotating = false;
            playerTransform = null;
            alignTrackingToPlayer = null;
        }

        internal enum RotationOriginEnum
        {

            Hips,

            ViewPoint,

            RightHand,

            LeftHand

        }

        /// <summary>
        ///     Do 4 rolls within 2 seconds
        /// </summary>
        /// <returns></returns>
        private IEnumerator BarrelRollCoroutine()
        {
            BarrelRolling = true;
            bool originalRotated = rotating;
            RotationOriginEnum originalOrigin = RotationOrigin;

            if (!originalRotated) Toggle();
            if (originalOrigin != RotationOriginEnum.Hips)
            {
                RotationOrigin = RotationOriginEnum.Hips;
                GrabOriginTransform();
            }

            var degreesCompleted = 0f;
            while (degreesCompleted < 720f)
            {
                yield return null;
                float currentRoll = 720 * Time.deltaTime;
                degreesCompleted += currentRoll;
                playerTransform.RotateAround(originTransform.position, usePlayerAxis ? -playerTransform.forward : -originTransform.forward, currentRoll);
                alignTrackingToPlayer?.Invoke();
            }

            yield return null;

            if (originalOrigin != RotationOriginEnum.Hips)
            {
                RotationOrigin = originalOrigin;
                GrabOriginTransform();
                yield return null;
            }

            if (!originalRotated) Toggle();
            BarrelRolling = false;
        }

        public void BarrelRoll()
        {
            if (WorldAllowed && !Utilities.GetStreamerMode)
                MelonCoroutines.Start(BarrelRollCoroutine());
        }

    }

}