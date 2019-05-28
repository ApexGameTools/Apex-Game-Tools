#pragma warning disable 1591
namespace Apex.Examples.Extensibility
{
    using Apex.Steering;
    using UnityEngine;

    /// <summary>
    /// Example of an <see cref="IMoveUnits"/> implementation.
    /// </summary>
    [AddComponentMenu("Apex/Examples/Extensibility/Character Controller Mover", 1002)]
    public class CharacterControllerMover : MonoBehaviour, IMoveUnits
    {
        private CharacterController _controller;
        private Transform _transform;

        public void Move(Vector3 velocity, float deltaTime)
        {
            _controller.Move(velocity * deltaTime);
        }

        public void Rotate(Vector3 targetOrientation, float angularSpeed, float deltaTime)
        {
            _transform.forward = Vector3.RotateTowards(_transform.forward, targetOrientation, angularSpeed * deltaTime, 0f);
        }

        public void Stop()
        {
            /* NOOP */
        }

        private void Awake()
        {
            _controller = this.GetComponent<CharacterController>();
            _transform = this.transform;
        }
    }
}
