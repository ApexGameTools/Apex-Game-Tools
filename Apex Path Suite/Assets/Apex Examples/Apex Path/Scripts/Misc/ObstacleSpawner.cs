namespace Apex.Examples.Misc
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Utility component to spawn obstacles
    /// </summary>
    [AddComponentMenu("Apex/Examples/Obstacle Spawner", 1012)]
    public class ObstacleSpawner : MonoBehaviour
    {
        /// <summary>
        /// The obstacle mold for static obstacles
        /// </summary>
        public GameObject obstacleMold;

        /// <summary>
        /// The mold for moving/dynamic obstacles
        /// </summary>
        public GameObject movingMold;

        /// <summary>
        /// The scale range minimum
        /// </summary>
        public float scaleRangeMin = 0.5f;

        /// <summary>
        /// The scale range maximum
        /// </summary>
        public float scaleRangeMax = 3.0f;

        /// <summary>
        /// Whether to make it rain obstacles
        /// </summary>
        public bool spawnObstacleRain = false;

        /// <summary>
        /// The rain radius
        /// </summary>
        public float rainRadius = 30.0f;

        private void Start()
        {
            StartCoroutine(SpawnRain());
        }

        private IEnumerator SpawnRain()
        {
            while (this.spawnObstacleRain)
            {
                yield return new WaitForSeconds(1.0f);

                var pos = Random.insideUnitSphere * this.rainRadius;
                SpawnMoving(pos, 20.0f, false);
            }
        }

        /// <summary>
        /// Spawns a static obstacle.
        /// </summary>
        /// <param name="position">The position.</param>
        public void SpawnStatic(Vector3 position)
        {
            var obstacle = Instantiate(this.obstacleMold) as GameObject;

            var scaleX = Random.Range(this.scaleRangeMin, this.scaleRangeMax);
            var scaleY = Random.Range(this.scaleRangeMin, this.scaleRangeMax);
            var scaleZ = Random.Range(this.scaleRangeMin, this.scaleRangeMax);

            var rotationY = Random.Range(0.0f, 359.0f);

            obstacle.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
            position.y = obstacle.GetComponent<Renderer>().bounds.extents.y;

            obstacle.transform.position = position;
            obstacle.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, rotationY, 0.0f));
        }

        /// <summary>
        /// Spawns a moving obstacle.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="y">The y.</param>
        /// <param name="addForce">if set to <c>true</c> adds force.</param>
        public void SpawnMoving(Vector3 position, float? y, bool addForce)
        {
            var obstacle = Instantiate(this.movingMold) as GameObject;

            var scale = Random.Range(this.scaleRangeMin, this.scaleRangeMax);

            obstacle.transform.localScale = new Vector3(scale, scale, scale);

            if (y.HasValue)
            {
                position.y = y.Value;
            }
            else
            {
                var renderer = obstacle.GetComponent<Renderer>();
                var ybase = renderer.bounds.extents.y;
                position.y = Random.Range(ybase, ybase * 4);
            }

            obstacle.transform.position = position;

            if (addForce)
            {
                var rb = obstacle.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    var rotationY = Random.Range(0.0f, 359.0f);
                    obstacle.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, rotationY, 0.0f));

                    var forceFactor = Random.Range(100.0f, 500.0f);
                    rb.AddForce(obstacle.transform.forward * forceFactor, ForceMode.Force);
                }
            }
        }
    }
}
