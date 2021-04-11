namespace PlayerRotater.ControlSchemes.Interface
{

    using UnityEngine;

    internal interface IControlScheme
    {

        bool HandleInput(Transform playerTransform, Transform cameraTransform, float flyingSpeed, float rotationSpeed);

    }

}