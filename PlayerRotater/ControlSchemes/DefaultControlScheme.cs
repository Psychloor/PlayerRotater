namespace PlayerRotater.ControlSchemes
{

    using PlayerRotater.ControlSchemes.Interface;

    using UnityEngine;

    public class DefaultControlScheme : IControlScheme
    {

        private bool usePlayerAxis;

        /// <inheritdoc />
        bool IControlScheme.HandleInput(Transform playerTransform, Transform cameraTransform, float flyingSpeed, float rotationSpeed, Transform origin)
        {
            usePlayerAxis = RotationSystem.RotateAround == RotationSystem.RotateAroundEnum.Hips && RotationSystem.IsHumanoid;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                flyingSpeed *= 2f;
                rotationSpeed *= 2f;
            }

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
                    playerTransform.position += flyingSpeed * Time.deltaTime * cameraTransform.up;

                if (Input.GetKey(KeyCode.Q))
                    playerTransform.position -= flyingSpeed * Time.deltaTime * cameraTransform.up;

                // ----------------------------- Rotation -----------------------------

                // Pitch
                if (Input.GetKey(KeyCode.UpArrow))
                    Pitch(1f);

                if (Input.GetKey(KeyCode.DownArrow))
                    Pitch(-1f);

                if (Input.GetKey(KeyCode.RightArrow))
                {
                    // Ctrl Yaw, regular roll
                    if (Input.GetKey(KeyCode.LeftControl))
                        Yaw(1f);
                    else
                        Roll(1f);
                }

                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    if (Input.GetKey(KeyCode.LeftControl))
                        Yaw(-1f);
                    else
                        Roll(-1f);
                }

                alignTracking = true;
            }
            else
            {
                // ------------------------------ VR Flying ------------------------------
                if (Mathf.Abs(Input.GetAxis(InputAxes.LeftVertical)) > 0)
                {
                    playerTransform.position += flyingSpeed * Time.deltaTime * Input.GetAxis(InputAxes.LeftVertical) * cameraTransform.forward;
                    alignTracking = true;
                }

                if (Mathf.Abs(Input.GetAxis(InputAxes.LeftHorizontal)) > 0)
                {
                    playerTransform.position += flyingSpeed * Time.deltaTime * Input.GetAxis(InputAxes.LeftHorizontal) * cameraTransform.right;
                    alignTracking = true;
                }

                // Vertical movement VR
                if (Mathf.Abs(Input.GetAxis(InputAxes.RightTrigger)) >= 0.4f)
                {
                    playerTransform.position += flyingSpeed * Time.deltaTime * Input.GetAxis(InputAxes.RightVertical) * playerTransform.up;
                    alignTracking = true;
                }

                // ----------------------------- VR Rotation -----------------------------
                if (Mathf.Abs(Input.GetAxis(InputAxes.RightTrigger)) < .4f)
                {
                    // Pitch
                    if (Mathf.Abs(Input.GetAxis(InputAxes.RightVertical)) >= .1f)
                    {
                        Pitch(Input.GetAxis(InputAxes.RightVertical));
                        alignTracking = true;
                    }

                    // Roll
                    if (Mathf.Abs(Input.GetAxis(InputAxes.RightHorizontal)) >= .1f)
                    {
                        Roll(Input.GetAxis(InputAxes.RightHorizontal));
                        alignTracking = true;
                    }
                }
            }

            return alignTracking;
        }

    }

}