namespace Apex.Steering
{
    using Apex.LoadBalancing;
    using Apex.Services;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Basic steering scanner
    /// </summary>
    [AddComponentMenu("")]
    [ApexComponent("Steering")]
    public class BasicScanner : ExtendedMonoBehaviour, ILoadBalanced
    {
        /// <summary>
        /// The time in seconds between scanner scans
        /// </summary>
        public float scanInterval = 1.0f;

        /// <summary>
        /// The scan radius
        /// </summary>
        public float scanRadius = 6.0f;

        private Collider[] _units = new Collider[0];

        /// <summary>
        /// Gets the colliders of the units scanned
        /// </summary>
        /// <value>
        /// The units.
        /// </value>
        public Collider[] Units
        {
            get { return _units; }
        }

        bool ILoadBalanced.repeat
        {
            get { return this.enabled; }
        }

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            NavLoadBalancer.scanners.Add(this, this.scanInterval);
        }

        private void OnDisable()
        {
            NavLoadBalancer.scanners.Remove(this);
        }

        float? ILoadBalanced.ExecuteUpdate(float deltaTime, float nextInterval)
        {
            _units = UnityServices.physics.OverlapSphere(this.transform.position, this.scanRadius, Layers.units);

            return null;
        }
    }
}
