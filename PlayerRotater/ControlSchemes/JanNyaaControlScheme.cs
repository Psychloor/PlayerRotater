namespace PlayerRotater.ControlSchemes
{

    using PlayerRotater.ControlSchemes.Interface;

    using UnityEngine;

    public class JanNyaaControlScheme : IControlScheme
    {

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
                    playerTransform.Rotate(Vector3.right, rotationSpeed * Input.GetAxis(InputAxes.RightVertical) * Time.deltaTime);
                    alignTracking = true;
                }

                if (Mathf.Abs(Input.GetAxis(InputAxes.RightHorizontal)) >= .1f)
                {
                    // Roll if right trigger
                    playerTransform.Rotate(
                        Input.GetAxis(InputAxes.RightTrigger) >= 0.4f ? Vector3.back : Vector3.up,
                        rotationSpeed * Input.GetAxis(InputAxes.RightHorizontal) * Time.deltaTime);

                    alignTracking = true;
                }
            }

            return alignTracking;
        }

    }

}