namespace PlayerRotater
{

    using System;
    using System.Collections;

    using MelonLoader;

    using UnityEngine;

    using Object = UnityEngine.Object;

    public class RotationSystem
    {

        private const float FlyingSpeed = 5f;

        private const float RotationSpeed = 180f;

        internal static RotationSystem Instance;

        private Utilities.AlignTrackingToPlayerDelegate alignTrackingToPlayer;

        private Transform cameraTransform;

        private Vector3 originalGravity;

        private Transform playerTransform;

        private bool rotating;

        internal bool WorldAllowed;

        private RotationSystem()
        {
        }

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
                foreach (Transform transform in Object.FindObjectsOfType<Transform>())
                {
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

            playerTransform ??= Utilities.GetLocalVRCPlayer().transform;
            if (!rotating) originalGravity = Physics.gravity;
            try
            {
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
                MelonLogger.LogError("Error Toggling: " + e);
                rotating = false;
            }

            if (playerTransform)
                playerTransform.GetComponent<CharacterController>().enabled = !rotating;

            Utilities.LogDebug("Toggling end, new state: " + rotating);

            if (rotating) return;
            Physics.gravity = originalGravity;
            alignTrackingToPlayer?.Invoke();
        }

        internal void OnUpdate()
        {
            if (!rotating
                || !WorldAllowed) return;

            // ------------------------------ Flying ------------------------------
            // I believe i can fly, i believe i can't touch shit at all
            if (Input.GetKey(KeyCode.W)) playerTransform.position += FlyingSpeed * Time.deltaTime * cameraTransform.forward;

            if (Input.GetKey(KeyCode.A)) playerTransform.position -= FlyingSpeed * Time.deltaTime * cameraTransform.right;

            if (Input.GetKey(KeyCode.S)) playerTransform.position -= FlyingSpeed * Time.deltaTime * cameraTransform.forward;

            if (Input.GetKey(KeyCode.D)) playerTransform.position += FlyingSpeed * Time.deltaTime * cameraTransform.right;

            if (Input.GetKey(KeyCode.E)) playerTransform.position += FlyingSpeed * Time.deltaTime * playerTransform.up;

            if (Input.GetKey(KeyCode.Q)) playerTransform.position -= FlyingSpeed * Time.deltaTime * playerTransform.up;

            // VR Doesn't work
            /* if (Mathf.Abs(Input.GetAxis(InputAxes.LeftVertical)) > 0)
                 playerTransform.position += currentSpeed * Time.deltaTime * Input.GetAxis(InputAxes.LeftVertical) * cameraTransform.forward;
 
             if (Mathf.Abs(Input.GetAxis(InputAxes.LeftHorizontal)) > 0)
                 playerTransform.position += currentSpeed * Time.deltaTime * Input.GetAxis(InputAxes.LeftHorizontal) * cameraTransform.right;
 
             // Vertical movement VR
             if (Mathf.Abs(Input.GetAxis(InputAxes.RightTrigger)) >= 0.4f)
                 playerTransform.position += currentSpeed * Time.deltaTime * Input.GetAxis(InputAxes.RightVertical) * playerTransform.up;*/

            // ----------------------------- Rotation -----------------------------
            if (Input.GetKey(KeyCode.UpArrow)) playerTransform.Rotate(Vector3.right, RotationSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.DownArrow)) playerTransform.Rotate(Vector3.left, RotationSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.RightArrow)) playerTransform.Rotate(Vector3.back, RotationSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.LeftArrow))
                playerTransform.Rotate(Vector3.forward, RotationSpeed * Time.deltaTime);

            /* VR Doesn't work
            // so you won't fly up/down in vr while rotating
            if (Mathf.Abs(Input.GetAxis(InputAxes.RightTrigger)) < .4f)
            {
                if (Mathf.Abs(Input.GetAxis(InputAxes.RightVertical)) >= .1f)
                    playerTransform.Rotate(Vector3.right, RotationSpeed * Input.GetAxis(InputAxes.RightVertical) * Time.deltaTime);

                if (Mathf.Abs(Input.GetAxis(InputAxes.RightHorizontal)) >= .1f)
                    playerTransform.Rotate(Vector3.back, RotationSpeed * Input.GetAxis(InputAxes.RightHorizontal) * Time.deltaTime);
            }*/

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