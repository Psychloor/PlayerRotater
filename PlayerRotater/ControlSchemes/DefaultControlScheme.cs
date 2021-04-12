namespace PlayerRotater.ControlSchemes
{

    using PlayerRotater.ControlSchemes.Interface;

    using UnityEngine;

    public class DefaultControlScheme : IControlScheme
    {

        /// <inheritdoc />
        bool IControlScheme.HandleInput(Transform playerTransform, Transform cameraTransform, float flyingSpeed)
        {
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
                    RotationSystem.Instance.Pitch(1f);

                if (Input.GetKey(KeyCode.DownArrow))
                    RotationSystem.Instance.Pitch(-1f);

                if (Input.GetKey(KeyCode.RightArrow))
                {
                    // Ctrl Yaw, regular roll
                    if (Input.GetKey(KeyCode.LeftControl))
                        RotationSystem.Instance.Yaw(1f);
                    else
                        RotationSystem.Instance.Roll(1f);
                }

                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    if (Input.GetKey(KeyCode.LeftControl))
                        RotationSystem.Instance.Yaw(-1f);
                    else
                        RotationSystem.Instance.Roll(-1f);
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
                        RotationSystem.Instance.Pitch(Input.GetAxis(InputAxes.RightVertical));
                        alignTracking = true;
                    }

                    // Roll
                    if (Mathf.Abs(Input.GetAxis(InputAxes.RightHorizontal)) >= .1f)
                    {
                        RotationSystem.Instance.Roll(Input.GetAxis(InputAxes.RightHorizontal));
                        alignTracking = true;
                    }
                }
            }

            return alignTracking;
        }

    }

}