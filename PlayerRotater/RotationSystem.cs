namespace PlayerRotater
{

    using System;
    using System.Collections;

    using MelonLoader;

    using UnhollowerRuntimeLib;

    using UnityEngine;

    using Object = UnityEngine.Object;

    public class RotationSystem
    {

        internal static float FlyingSpeed = 5f;

        internal static float RotationSpeed = 180f;

        internal static bool NoClipFlying = true;

        internal static RotationSystem Instance;

        private Utilities.AlignTrackingToPlayerDelegate alignTrackingToPlayer;

        public Transform cameraTransform;

        private float currentSpeed = 10.0f;

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
            while (!Instance.cameraTransform)
            {
                yield return new WaitForSeconds(1f);
                foreach (Object component in Object.FindObjectsOfType(Il2CppType.Of<Transform>()))
                {
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

        internal void ToggleNoClip()
        {
            if (!playerTransform) return;
            CharacterController characterController = playerTransform.GetComponent<CharacterController>();
            if (!characterController) return;

            if (rotating && !Utilities.IsVR)
            {
                characterController.enabled = !NoClipFlying;
            }
            else if (!characterController.enabled)
                characterController.enabled = true;
            
            if (Utilities.IsVR)
                Utilities.GetLocalVRCPlayer()?.prop_VRCPlayerApi_0.Immobilize(rotating);
        }

        internal void OnUpdate()
        {
            if (!rotating
                || !WorldAllowed) return;

            bool WeRotated = false;

            if (!Utilities.IsVR)
            {
                // ------------------------------ Flying ------------------------------
                if (Input.GetKey(KeyCode.W))
                    playerTransform.position += FlyingSpeed * Time.deltaTime * cameraTransform.forward;

                if (Input.GetKey(KeyCode.A))
                    playerTransform.position -= FlyingSpeed * Time.deltaTime * cameraTransform.right;

                if (Input.GetKey(KeyCode.S))
                    playerTransform.position -= FlyingSpeed * Time.deltaTime * cameraTransform.forward;

                if (Input.GetKey(KeyCode.D))
                    playerTransform.position += FlyingSpeed * Time.deltaTime * cameraTransform.right;

                if (Input.GetKey(KeyCode.E))
                    playerTransform.position += FlyingSpeed * Time.deltaTime * playerTransform.up;

                if (Input.GetKey(KeyCode.Q))
                    playerTransform.position -= FlyingSpeed * Time.deltaTime * playerTransform.up;



                // ----------------------------- Rotation -----------------------------
                if (Input.GetKey(KeyCode.UpArrow))
                    playerTransform.Rotate(Vector3.right, RotationSpeed * Time.deltaTime);

                if (Input.GetKey(KeyCode.DownArrow))
                    playerTransform.Rotate(Vector3.left, RotationSpeed * Time.deltaTime);

                if (Input.GetKey(KeyCode.RightArrow))
                    playerTransform.Rotate(Vector3.back, RotationSpeed * Time.deltaTime);

                if (Input.GetKey(KeyCode.LeftArrow))
                    playerTransform.Rotate(Vector3.forward, RotationSpeed * Time.deltaTime);

                WeRotated = true;
            } else
            {
                // ------------------------------ VR Flying ------------------------------
                if (Mathf.Abs(Input.GetAxis(InputAxes.LeftVertical)) > 0)
                {
                    playerTransform.position += currentSpeed * Time.deltaTime * Input.GetAxis(InputAxes.LeftVertical) * cameraTransform.forward;
                    WeRotated = true;
                }


                if (Mathf.Abs(Input.GetAxis(InputAxes.LeftHorizontal)) > 0)
                {
                    playerTransform.position += currentSpeed * Time.deltaTime * Input.GetAxis(InputAxes.LeftHorizontal) * cameraTransform.right;
                    WeRotated = true;
                }

                // Vertical movement VR
                if (Mathf.Abs(Input.GetAxis(InputAxes.RightTrigger)) >= 0.4f)
                {
                    playerTransform.position += currentSpeed * Time.deltaTime * Input.GetAxis(InputAxes.RightVertical) * playerTransform.up;
                    WeRotated = true;
                }


                // ----------------------------- VR Rotation -----------------------------
                if (Mathf.Abs(Input.GetAxis(InputAxes.RightTrigger)) < .4f)
                {
                    if (Mathf.Abs(Input.GetAxis(InputAxes.RightVertical)) >= .1f)
                    {
                        playerTransform.Rotate(Vector3.right, RotationSpeed * Input.GetAxis(InputAxes.RightVertical) * Time.deltaTime);
                        WeRotated = true;
                    }

                    if (Mathf.Abs(Input.GetAxis(InputAxes.RightHorizontal)) >= .1f)
                    {
                        playerTransform.Rotate(Vector3.back, RotationSpeed * Input.GetAxis(InputAxes.RightHorizontal) * Time.deltaTime);
                        WeRotated = true;
                    }
                }
            }

            if (WeRotated)
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