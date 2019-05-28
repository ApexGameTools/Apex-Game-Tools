namespace Apex.Tutorials
{
    using System.Collections;
    using Apex.Examples.Misc;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// A utility that opens and closes a door on the press of a button.
    /// </summary>
    [AddComponentMenu("Apex/Tutorials/Open Door on Button", 1047)]
    public class OpenDoorOnButton : MonoBehaviour
    {
        /// <summary>
        /// The speed in seconds by which the door opens / closes
        /// </summary>
        public float speedInSeconds = 1.0f;

        private IDynamicObstacle _obstacle;
        private Slider _slider;

        private void Awake()
        {
            _obstacle = this.GetComponent<DynamicObstacle>();

            var b = this.GetComponent<Renderer>().bounds;

            //move distance is a bit too long if the door is axis aligned, but it'll do for this example
            _slider = new Slider((b.max - b.min).magnitude, this.speedInSeconds);
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 100, 50), "Open"))
            {
                StartCoroutine(Slide(1));
            }

            if (GUI.Button(new Rect(120, 10, 100, 50), "Close"))
            {
                StartCoroutine(Slide(-1));
            }
        }

        private IEnumerator Slide(int dir)
        {
            if (!_slider.SetDirection(dir))
            {
                yield break;
            }

            while (_slider.MoveNext(this.transform))
            {
                yield return null;
            }

            //We only update the door obstacle status when it comes to a rest in either its open or closed state to avoid unnecessary replans
            _obstacle.ActivateUpdates(null, false);
        }
    }
}
