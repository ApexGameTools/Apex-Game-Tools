namespace Apex.Examples.Misc
{
    using System.Collections.Generic;
    using Apex.Steering.Components;
    using UnityEngine;

    /// <summary>
    /// A component that controls the speed of all <see cref="Apex.Steering.Components.HumanoidSpeedComponent"/> in the scene
    /// </summary>
    [AddComponentMenu("Apex/Examples/Omni Speed Controller", 1013)]
    public class OmniSpeedController : MonoBehaviour
    {
        private HumanoidSpeedComponent[] _speedControllers;

        private void Awake()
        {
            _speedControllers = Resources.FindObjectsOfTypeAll<HumanoidSpeedComponent>();
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 100, 50), "Crawl"))
            {
                foreach (var c in _speedControllers)
                {
                    c.Crawl();
                }
            }

            if (GUI.Button(new Rect(10, 70, 100, 50), "Walk"))
            {
                foreach (var c in _speedControllers)
                {
                    c.Walk();
                }
            }

            if (GUI.Button(new Rect(10, 130, 100, 50), "Run"))
            {
                foreach (var c in _speedControllers)
                {
                    c.Run();
                }
            }
        }
    }
}
