namespace Apex.Examples.Input
{
    using System;

    /// <summary>
    /// An example of defining input buttons
    /// </summary>
    public static class InputButton
    {
        /// <summary>
        /// The select group modifier
        /// </summary>
        public const string SelectGroupModifier = "SelectGroupModifier";

        /// <summary>
        /// The assign group modifier
        /// </summary>
        public const string AssignGroupModifier = "AssignGroupModifier";

        /// <summary>
        /// The set destination button
        /// </summary>
        public const string SetDestination = "SetDestination";

        /// <summary>
        /// The set destination way point modifier button
        /// </summary>
        public const string SetDestinationWaypointModifier = "SetDestinationWaypointModifier";

        /// <summary>
        /// The select button
        /// </summary>
        public const string Select = "Select";

        /// <summary>
        /// The select append modifier
        /// </summary>
        public const string SelectAppendModifier = "SelectAppendModifier";

        private static readonly string[] _unitSelects;

        static InputButton()
        {
            _unitSelects = new string[9];
            for (int i = 1; i < 10; i++)
            {
                _unitSelects[i - 1] = string.Concat("SelectUnit", i);
            }
        }

        /// <summary>
        /// Create a button by suffixing an index to the unit selection button base.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The button</returns>
        /// <exception cref="System.ArgumentException">Range must be 0 to 9</exception>
        public static string UnitSelect(int index)
        {
            if (index < 0 || index >= _unitSelects.Length)
            {
                throw new ArgumentException("Range must be 0 to 9");
            }

            return _unitSelects[index];
        }
    }
}
