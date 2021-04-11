namespace PlayerRotater.ControlSchemes
{

    using PlayerRotater.ControlSchemes.Interface;

    using UnityEngine;

    public class DefaultControlScheme : IControlScheme
    {

        /// <inheritdoc />
        bool IControlScheme.HandleInput(Transform playerTransform, Transform cameraTransform, float flyingSpeed, float rotationSpeed)
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
                        playerTransform.Rotate(Vector3.right, rotationSpeed * Input.GetAxis(InputAxes.RightVertical) * Time.deltaTime);
                        alignTracking = true;
                    }

                    // Roll
                    if (Mathf.Abs(Input.GetAxis(InputAxes.RightHorizontal)) >= .1f)
                    {
                        playerTransform.Rotate(Vector3.back, rotationSpeed * Input.GetAxis(InputAxes.RightHorizontal) * Time.deltaTime);
                        alignTracking = true;
                    }
                }
            }

            return alignTracking;
        }

    }

}