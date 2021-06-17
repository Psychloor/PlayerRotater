namespace PlayerRotater.ControlSchemes
{

    using PlayerRotater.ControlSchemes.Interface;

    using UnityEngine;

    public class DefaultControlScheme : IControlScheme
    {

        /// <inheritdoc />
        bool IControlScheme.HandleInput(Transform playerTransform, Transform cameraTransform)
        {
            var alignTracking = false;
            if (!Utilities.IsInVR)
            {
                if (!Utilities.AnyActionMenuesOpen())
                {
                    // ------------------------------ Flying ------------------------------
                    if (Input.GetKey(KeyCode.W))
                        RotationSystem.Instance.Fly(1f, cameraTransform.forward);

                    if (Input.GetKey(KeyCode.A))
                        RotationSystem.Instance.Fly(1f, -cameraTransform.right);

                    if (Input.GetKey(KeyCode.S))
                        RotationSystem.Instance.Fly(1f, -cameraTransform.forward);

                    if (Input.GetKey(KeyCode.D))
                        RotationSystem.Instance.Fly(1f, cameraTransform.right);

                    if (Input.GetKey(KeyCode.E))
                        RotationSystem.Instance.Fly(1f, cameraTransform.up);

                    if (Input.GetKey(KeyCode.Q))
                        RotationSystem.Instance.Fly(1f, -cameraTransform.up);

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
                if (!Utilities.AnyActionMenuesOpen())
                {
                    // ------------------------------ VR Flying ------------------------------
                    if (Mathf.Abs(Input.GetAxis(InputAxes.LeftVertical)) > 0.1f)
                    {
                        RotationSystem.Instance.Fly(Input.GetAxis(InputAxes.LeftVertical), cameraTransform.forward);
                        alignTracking = true;
                    }

                    if (Mathf.Abs(Input.GetAxis(InputAxes.LeftHorizontal)) > 0.1f)
                    {
                        RotationSystem.Instance.Fly(Input.GetAxis(InputAxes.LeftHorizontal), cameraTransform.right);
                        alignTracking = true;
                    }

                    // Vertical movement VR
                    if (Mathf.Abs(Input.GetAxis(InputAxes.RightTrigger)) >= 0.4f)
                    {
                        RotationSystem.Instance.Fly(Input.GetAxis(InputAxes.LeftVertical), playerTransform.up);
                        alignTracking = true;
                    }

                    if (!RotationSystem.LockRotation)

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
            }

            return alignTracking;
        }

    }

}