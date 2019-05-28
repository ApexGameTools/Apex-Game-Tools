namespace Apex.Examples.Misc
{
    using UnityEngine;

    /// <summary>
    /// A component that destroys all that exits its collider.
    /// </summary>
    [AddComponentMenu("Apex/Examples/Destroy On Exit", 1002)]
    public class DestroyOnExit : MonoBehaviour
    {
        private void OnTriggerExit(Collider other)
        {
            Destroy(other.gameObject);
        }
    }
}
