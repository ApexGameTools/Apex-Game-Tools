namespace Apex.Tutorials
{
    using UnityEngine;

    /// <summary>
    /// Moves a transform between two points.
    /// </summary>
    [AddComponentMenu("Apex/Tutorials/SimpleMoveLoop", 1048)]
    public class SimpleMoveLoop : MonoBehaviour
    {
        /// <summary>
        /// The first position
        /// </summary>
        public Vector3 position1 = new Vector3(-5, 0, 0);

        /// <summary>
        /// The second position
        /// </summary>
        public Vector3 position2 = new Vector3(5, 0, 0);

        /// <summary>
        /// The speed
        /// </summary>
        public float speed = 1;

        /// <summary>
        /// The tolerance (when to switch directions)
        /// </summary>
        public float tolerance = 0.1f;

        private Vector3 _currentDestination;
        private Transform _transform;
        private Rigidbody _rigidbody;

        private void Start()
        {
            _transform = transform;
            _currentDestination = position1;
            _rigidbody = this.GetComponent<Rigidbody>();
        }

        private void Update()
        {
            var distance = CalculateDistanceInXZPlane(_currentDestination, _transform.position);

            if (distance < tolerance)
            {
                _currentDestination = SelectNextDestination();
            }

            var d = (_currentDestination - _transform.position);

            var direction = new Vector3(d.x, 0, d.z).normalized;

            var velocity = direction * speed;
            _rigidbody.velocity = velocity;
        }

        private Vector3 SelectNextDestination()
        {
            var distance1 = CalculateDistanceInXZPlane(position1, _transform.position);
            var distance2 = CalculateDistanceInXZPlane(position2, _transform.position);

            if (distance1 < distance2)
            {
                return position2;
            }

            return position1;
        }

        private float CalculateDistanceInXZPlane(Vector3 p1, Vector3 p2)
        {
            return Mathf.Sqrt(Mathf.Pow((p2.x - p1.x), 2) + Mathf.Pow((p2.z - p1.z), 2));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(position1, 0.25f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(position2, 0.25f);
        }
    }
}
