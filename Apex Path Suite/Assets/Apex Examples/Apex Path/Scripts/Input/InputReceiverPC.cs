namespace Apex.Examples.Input
{
    using Apex.Examples.Extensibility;
    using Apex.Examples.Misc;
    using Apex.Input;
    using Apex.Services;
    using UnityEngine;

    /// <summary>
    /// Example input receiver using proper buttons that interact with the Unity InputManager
    /// </summary>
    [AddComponentMenu("Apex/Examples/InputReceiverPC", 1009)]
    [InputReceiver]
    public partial class InputReceiverPC : MonoBehaviour
    {
        private InputController _inputController;
        private SelectionRectangleComponent _selectRectangle;
        private Vector3 _lastSelectDownPos;

        private void Awake()
        {
            _inputController = new InputController();
            _selectRectangle = this.GetComponentInChildren<SelectionRectangleComponent>();

            if (_selectRectangle == null)
            {
                Debug.LogWarning("Missing SelectionRectangleComponent, this is required by the input receiver to handle unit selection.");
            }

            if (Application.platform != RuntimePlatform.WindowsPlayer &&
#if !(UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_2017)
                Application.platform != RuntimePlatform.WindowsWebPlayer &&
#endif
                Application.platform != RuntimePlatform.WindowsEditor)
            {
                Debug.LogWarning("The InputReceiverPC only works on Windows.");
            }
        }

        private void Update()
        {
            Movement();

            Selection();

            Misc();

            Steer();
        }

        partial void Steer();

        private void Movement()
        {
            if (Input.GetButtonUp(InputButton.SetDestination))
            {
                var setWaypoint = Input.GetButton(InputButton.SetDestinationWaypointModifier);

                _inputController.SetDestination(Input.mousePosition, setWaypoint);
            }
        }

        private void Selection()
        {
            if (_selectRectangle == null)
            {
                return;
            }

            var selectAppend = Input.GetButton(InputButton.SelectAppendModifier);

            if (Input.GetButtonDown(InputButton.Select))
            {
                _lastSelectDownPos = Input.mousePosition;
                _selectRectangle.StartSelect();
                return;
            }

            if (Input.GetButton(InputButton.Select))
            {
                if (_selectRectangle.HasSelection(_lastSelectDownPos, Input.mousePosition))
                {
                    _inputController.SelectUnitRangeTentative(_lastSelectDownPos, Input.mousePosition, selectAppend);
                }

                return;
            }

            if (Input.GetButtonUp(InputButton.Select))
            {
                if (_selectRectangle.HasSelection(_lastSelectDownPos, Input.mousePosition))
                {
                    _inputController.SelectUnitRange(_lastSelectDownPos, Input.mousePosition, selectAppend);
                }
                else
                {
                    _inputController.SelectUnit(_lastSelectDownPos, selectAppend);
                }

                _selectRectangle.EndSelect();
                return;
            }

            var selectGroup = Input.GetButton(InputButton.SelectGroupModifier);
            var assignGroup = Input.GetButton(InputButton.AssignGroupModifier);

            for (int index = 0; index < 5; index++)
            {
                if (Input.GetButtonUp(InputButton.UnitSelect(index)))
                {
                    if (selectGroup)
                    {
                        _inputController.SelectGroup(index);
                    }
                    else if (assignGroup)
                    {
                        _inputController.AssignGroup(index);
                    }
                    else
                    {
                        _inputController.SelectUnit(index, false);
                    }
                }
            }
        }

        private void Misc()
        {
            if (Input.GetKeyUp(KeyCode.Q))
            {
                var spawner = GetComponent<ObstacleSpawner>();
                if (spawner != null)
                {
                    var pos = UnityServices.mainCamera.ScreenToGroundPoint(Input.mousePosition);
                    spawner.SpawnStatic(pos);
                }
            }

            if (Input.GetKeyUp(KeyCode.W))
            {
                var spawner = GetComponent<ObstacleSpawner>();
                if (spawner != null)
                {
                    var pos = UnityServices.mainCamera.ScreenToGroundPoint(Input.mousePosition);
                    spawner.SpawnMoving(pos, null, true);
                }
            }
        }
    }
}
