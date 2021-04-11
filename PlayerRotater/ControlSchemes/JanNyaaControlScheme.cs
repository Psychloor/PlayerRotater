namespace PlayerRotater.ControlSchemes
{

    using PlayerRotater.ControlSchemes.Interface;

    using UnityEngine;

    public class JanNyaaControlScheme : IControlScheme
    {

        private bool usePlayerAxis;

        /// <inheritdoc />
        bool IControlScheme.HandleInput(Transform playerTransform, Transform cameraTransform, float flyingSpeed, float rotationSpeed, Transform origin)
        {
            usePlayerAxis = RotationSystem.RotateAround == RotationSystem.RotateAroundEnum.Hips && RotationSystem.IsHumanoid;

            void Pitch(float amount)
            {
                if (RotationSystem.InvertPitch) amount *= -1;
                playerTransform.RotateAround(origin.position, usePlayerAxis ? playerTransform.right : origin.right, amount * rotationSpeed * Time.deltaTime);
            }

            void Yaw(float amount)
            {
                playerTransform.RotateAround(origin.position, usePlayerAxis ? playerTransform.up : origin.up, amount * rotationSpeed * Time.deltaTime);
            }

            void Roll(float amount)
            {
                playerTransform.RotateAround(
                    origin.position,
                    usePlayerAxis ? playerTransform.forward : origin.forward,
                    -amount * rotationSpeed * Time.deltaTime);
            }

            var alignTracking = false;
            if (!Utilities.IsVR)
            {
                // ------------------------------ Flying ------------------------------
                if (Input.GetKey(KeyCode.W))
                    playerTransform.position += flyingSpeed * Time.deltaTime * cameraTransform.forward;

                if (Input.GetKey(KeyCode.A))
                    playerTransform.position -= flyingSpeed * Time.deltaTime * cameraTransform.right;

                if (Input.GetKey(KeyCode.S))
                    playerTransform.position -= flyingSpeed * Time.deltaTime * cameraTransform.forward;

                if (Input.GetKey(KeyCode.D))
                    playerTransform.position += flyingSpeed * Time.deltaTime * cameraTransform.right;

                if (Input.GetKey(KeyCode.E))
                    playerTransform.position += flyingSpeed * Time.deltaTime * playerTransform.up;

                if (Input.GetKey(KeyCode.Q))
                    playerTransform.position -= flyingSpeed * Time.deltaTime * playerTransform.up;

                // ----------------------------- Rotation -----------------------------
                if (Input.GetKey(KeyCode.UpArrow))
                    playerTransform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);

                if (Input.GetKey(KeyCode.DownArrow))
                    playerTransform.Rotate(Vector3.left, rotationSpeed * Time.deltaTime);

                if (Input.GetKey(KeyCode.RightArrow))
                    playerTransform.Rotate(Vector3.back, rotationSpeed * Time.deltaTime);

                if (Input.GetKey(KeyCode.LeftArrow))
                    playerTransform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

                alignTracking = true;
            }
            else
            {
                // ------------------------------ VR Flying ------------------------------
                if (Mathf.Abs(Input.GetAxis(InputAxes.LeftVertical)) >= 0.1f)
                {
                    // Vertical if holding left trigger
                    if (Input.GetAxis(InputAxes.LeftTrigger) >= 0.4f)
                        playerTransform.position += Input.GetAxis(InputAxes.LeftVertical) * flyingSpeed * Time.deltaTime * playerTransform.up;
                    else playerTransform.position += Input.GetAxis(InputAxes.LeftVertical) * flyingSpeed * Time.deltaTime * playerTransform.forward;

                    alignTracking = true;
                }

                if (Mathf.Abs(Input.GetAxis(InputAxes.LeftHorizontal)) >= 0.1f)
                {
                    playerTransform.position += Input.GetAxis(InputAxes.LeftVertical) * flyingSpeed * Time.deltaTime * playerTransform.right;
                    alignTracking = true;
                }

                // ----------------------------- VR Rotation -----------------------------

                // Pitch
                if (Mathf.Abs(Input.GetAxis(InputAxes.RightVertical)) >= .1f)
                {
                    Pitch(Input.GetAxis(InputAxes.RightVertical));
                    alignTracking = true;
                }

                if (Mathf.Abs(Input.GetAxis(InputAxes.RightHorizontal)) >= .1f)
                {
                    // Roll if right trigger otherwise yaw
                    if (Input.GetAxis(InputAxes.RightTrigger) >= .4f) Roll(Input.GetAxis(InputAxes.RightHorizontal));
                    else Yaw(Input.GetAxis(InputAxes.RightHorizontal));

                    alignTracking = true;
                }
            }

            return alignTracking;
        }

    }

}