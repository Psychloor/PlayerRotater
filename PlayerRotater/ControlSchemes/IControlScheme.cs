namespace PlayerRotater.ControlSchemes
{

    using UnityEngine;

    internal interface IControlScheme
    {

        bool HandleInput(Transform playerTransform, Transform cameraTransform, float flyingSpeed, float rotationSpeed);

    }

}