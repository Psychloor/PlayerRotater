namespace PlayerRotater.ControlSchemes
{

    using PlayerRotater.ControlSchemes.Interface;

    using UnityEngine;

    public class JanNyaaControlScheme : IControlScheme
    {

        /// <inheritdoc />
        bool IControlScheme.HandleInput(Transform playerTransform, Transform cameraTransform, float flyingSpeed)
        {
            var alignTracking = false;
            if (!Utilities.IsVR)
            {
                if (!Utilities.ActionMenuesOpen())
                {
                    // ------------------------------ Flying ------------------------------
                    if (Input.GetKey(KeyCode.W))
                        playerTransform.position += flyingSpeed * Time.deltaTime * playerTransform.forward;

                    if (Input.GetKey(KeyCode.A))
                        playerTransform.position -= flyingSpeed * Time.deltaTime * playerTransform.right;

                    if (Input.GetKey(KeyCode.S))
                        playerTransform.position -= flyingSpeed * Time.deltaTime * playerTransform.forward;

                    if (Input.GetKey(KeyCode.D))
                        playerTransform.position += flyingSpeed * Time.deltaTime * playerTransform.right;

                    if (Input.GetKey(KeyCode.E))
                        playerTransform.position += flyingSpeed * Time.deltaTime * playerTransform.up;

                    if (Input.GetKey(KeyCode.Q))
                        playerTransform.position -= flyingSpeed * Time.deltaTime * playerTransform.up;

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
                }

                alignTracking = true;
            }
            else
            {
                if (!Utilities.ActionMenuesOpen())
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
                        playerTransform.position += Input.GetAxis(InputAxes.LeftHorizontal) * flyingSpeed * Time.deltaTime * playerTransform.right;
                        alignTracking = true;
                    }

                    // ----------------------------- VR Rotation -----------------------------

                    // Pitch
                    if (Mathf.Abs(Input.GetAxis(InputAxes.RightVertical)) >= .1f)
                    {
                        if (Input.GetAxis(InputAxes.RightTrigger) >= .4f)
                        {
                            RotationSystem.Instance.Pitch(Input.GetAxis(InputAxes.RightVertical));
                            alignTracking = true;
                        }
                    }

                    if (Mathf.Abs(Input.GetAxis(InputAxes.RightHorizontal)) >= .1f)
                    {
                        // Roll if right trigger otherwise yaw
                        if (Input.GetAxis(InputAxes.RightTrigger) >= .4f) RotationSystem.Instance.Roll(Input.GetAxis(InputAxes.RightHorizontal));
                        else RotationSystem.Instance.Yaw(Input.GetAxis(InputAxes.RightHorizontal));

                        alignTracking = true;
                    }
                }
            }

            return alignTracking;
        }

    }

}