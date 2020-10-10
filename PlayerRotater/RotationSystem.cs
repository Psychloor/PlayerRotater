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

        // field stored for current speed to add.
        // gets updated in updatemethod. so don't touch
        private float currentSpeed = 10.0f;

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

        // bit weird but i've gotten some errors few times when it bugged out a bit
        internal void Toggle()
        {
            if (!WorldAllowed) return;
            if (RoomManagerBase.field_Internal_Static_ApiWorld_0 == null
                || RoomManagerBase.field_Internal_Static_ApiWorldInstance_0 == null) return;

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
                    Physics.gravity = originalGravity;
                }
            }
            catch
            {
                rotating = false;
            }

            if (playerTransform)
                playerTransform.GetComponent<CharacterController>().enabled = !rotating;

            if (rotating) return;
            Physics.gravity = originalGravity;
            alignTrackingToPlayer?.Invoke();
        }

        internal void OnUpdate()
        {
            if (!rotating
                || !WorldAllowed) return;

            // ------------------------------ Flying ------------------------------
            currentSpeed = FlyingSpeed;

            // I believe i can fly, i believe i can't touch shit at all
            if (Input.GetKey(KeyCode.W)) playerTransform.position += currentSpeed * Time.deltaTime * cameraTransform.forward;

            if (Input.GetKey(KeyCode.A)) playerTransform.position -= currentSpeed * Time.deltaTime * cameraTransform.right;

            if (Input.GetKey(KeyCode.S)) playerTransform.position -= currentSpeed * Time.deltaTime * cameraTransform.forward;

            if (Input.GetKey(KeyCode.D)) playerTransform.position += currentSpeed * Time.deltaTime * cameraTransform.right;

            if (Input.GetKey(KeyCode.E)) playerTransform.position += currentSpeed * Time.deltaTime * playerTransform.up;

            if (Input.GetKey(KeyCode.Q)) playerTransform.position -= currentSpeed * Time.deltaTime * playerTransform.up;

            if (Mathf.Abs(Input.GetAxis(InputAxes.LeftVertical)) > 0)
                playerTransform.position += currentSpeed * Time.deltaTime * Input.GetAxis(InputAxes.LeftVertical) * cameraTransform.forward;

            if (Mathf.Abs(Input.GetAxis(InputAxes.LeftHorizontal)) > 0)
                playerTransform.position += currentSpeed * Time.deltaTime * Input.GetAxis(InputAxes.LeftHorizontal) * cameraTransform.right;

            // Vertical movement VR
            if (Mathf.Abs(Input.GetAxis(InputAxes.RightTrigger)) >= 0.4f)
                playerTransform.position += currentSpeed * Time.deltaTime * Input.GetAxis(InputAxes.RightVertical) * playerTransform.up;

            // ----------------------------- Rotation -----------------------------
            if (Input.GetKey(KeyCode.UpArrow)) playerTransform.Rotate(Vector3.right, RotationSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.DownArrow)) playerTransform.Rotate(Vector3.left, RotationSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.RightArrow)) playerTransform.Rotate(Vector3.back, RotationSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.LeftArrow))
                playerTransform.Rotate(Vector3.forward, RotationSpeed * Time.deltaTime);

            // so you won't fly up/down in vr while rotating
            if (Mathf.Abs(Input.GetAxis(InputAxes.RightTrigger)) < .4f)
            {
                if (Mathf.Abs(Input.GetAxis(InputAxes.RightVertical)) >= .1f)
                    playerTransform.Rotate(Vector3.right, RotationSpeed * Input.GetAxis(InputAxes.RightVertical) * Time.deltaTime);

                if (Mathf.Abs(Input.GetAxis(InputAxes.RightHorizontal)) >= .1f)
                    playerTransform.Rotate(Vector3.back, RotationSpeed * Input.GetAxis(InputAxes.RightHorizontal) * Time.deltaTime);
            }

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