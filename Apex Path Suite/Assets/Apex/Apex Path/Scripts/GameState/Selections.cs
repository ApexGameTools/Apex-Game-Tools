/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.GameState
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Apex;
    using Apex.DataStructures;
    using Apex.Messages;
    using Apex.Services;
    using Apex.Steering.Messages;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// Encapsulates all things to do with unit selection.
    /// </summary>
    public sealed class Selections
    {
        private static readonly IGrouping<IUnitFacade> _emptyGroup = new EmptyGrouping();
        private IGrouping<IUnitFacade> _selected;
        private IndexableSet<IUnitFacade> _selectableUnits;
        private IDictionary<int, IGrouping<IUnitFacade>> _groups;
        private List<int> _removalBuffer = new List<int>(2);

        /// <summary>
        /// Initializes a new instance of the <see cref="Selections"/> class.
        /// </summary>
        public Selections()
        {
            _selected = _emptyGroup;
            _selectableUnits = new IndexableSet<IUnitFacade>();
            _groups = new Dictionary<int, IGrouping<IUnitFacade>>();
        }

        /// <summary>
        /// Gets the currently selected units.
        /// </summary>
        public IGrouping<IUnitFacade> selected
        {
            get { return _selected; }
        }

        /// <summary>
        /// Gets the selectable units.
        /// </summary>
        public IIterable<IUnitFacade> selectableUnits
        {
            get { return _selectableUnits; }
        }

        /// <summary>
        /// Registers the a unit as selectable.
        /// </summary>
        /// <param name="unit">The unit.</param>
        public void RegisterSelectable(IUnitFacade unit)
        {
            _selectableUnits.Add(unit);
        }

        /// <summary>
        /// Unregisters the a unit as selectable.
        /// </summary>
        /// <param name="unit">The unit.</param>
        public void UnregisterSelectable(IUnitFacade unit)
        {
            //The unit will be removed from its transient group so we do not need to do that here
            unit.isSelected = false;
            _selectableUnits.Remove(unit);

            //No need to create an enumerator if the groups dict is empty
            if (_groups.Count > 0)
            {
                foreach (var grpEntry in _groups)
                {
                    grpEntry.Value.Remove(unit);
                    if (grpEntry.Value.memberCount == 0)
                    {
                        _removalBuffer.Add(grpEntry.Key);
                    }
                }

                for (int i = 0; i < _removalBuffer.Count; i++)
                {
                    _groups.Remove(_removalBuffer[i]);
                }

                _removalBuffer.Clear();
            }
        }

        /// <summary>
        /// Tentatively selects all selectable units inside the specified bounds.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <param name="append">if set to <c>true</c> the selection will append to the current selection.</param>
        public void SelectUnitsAsPendingIn(PolygonXZ bounds, bool append)
        {
            int count = _selectableUnits.count;
            for (int i = 0; i < count; i++)
            {
                var unit = _selectableUnits[i];
                bool selectPending = (append && unit.isSelected) || bounds.Contains(unit.position);
                unit.MarkSelectPending(selectPending);
            }
        }

        /// <summary>
        /// Selects all selectable units inside the specified bounds.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <param name="append">if set to <c>true</c> the selection will append to the current selection.</param>
        public void SelectUnitsIn(PolygonXZ bounds, bool append)
        {
            var selected = _selectableUnits.Where(u => bounds.Contains(u.position));
            Select(append, selected);
        }

        /// <summary>
        /// Selects the specified units.
        /// </summary>
        /// <param name="append">if set to <c>true</c> the selection will append to the current selection.</param>
        /// <param name="units">The units.</param>
        public void Select(bool append, params IUnitFacade[] units)
        {
            Select(append, (IEnumerable<IUnitFacade>)units);
        }

        /// <summary>
        /// Selects the specified units.
        /// </summary>
        /// <param name="append">if set to <c>true</c> the selection will append to the current selection.</param>
        /// <param name="units">The units.</param>
        public void Select(bool append, IEnumerable<IUnitFacade> units)
        {
            IEnumerable<IUnitFacade> newSelections;

            if (_selected.memberCount == 0)
            {
                //This is we have an empty group. In that case recreate all.
                newSelections = units;
                _selected = GroupingManager.CreateGrouping(units);
            }
            else if (append)
            {
                newSelections = units.Except(_selected.All()).ToArray();
                _selected.Add(newSelections);
            }
            else
            {
                //If we select all the same units as before and maybe some more, we just add those additional ones
                //If the selection does not include all from before, we create a new grouping
                var deselected = _selected.All().Except(units);
                if (deselected.Any())
                {
                    DeselectAll();
                    newSelections = units;
                    _selected = GroupingManager.CreateGrouping(units);
                }
                else
                {
                    newSelections = units.Except(_selected.All()).ToArray();
                    _selected.Add(newSelections);
                }
            }

            newSelections.Apply(u => u.isSelected = true);

            PostUnitsSelectedMessage(_selected);
        }

        /// <summary>
        /// Selects the unit at the specified unit index.
        /// </summary>
        /// <param name="unitIndex">Index of the unit.</param>
        /// <param name="toogle">if set to <c>true</c> this will toggle the selection, i.e. if selected it will deselect and vice versa.</param>
        /// <returns>The selected unit</returns>
        public IUnitFacade Select(int unitIndex, bool toogle)
        {
            if (unitIndex >= _selectableUnits.count || unitIndex < 0)
            {
                return null;
            }

            var unit = _selectableUnits[unitIndex];

            if (toogle)
            {
                DeselectAll();
                ToggleSelected(unit, true);
            }
            else
            {
                Select(false, unit);
            }

            return unit;
        }

        /// <summary>
        /// Toggles the selected state of a unit. Actually it only toggles when <paramref name="append"/> is true, otherwise it will select the unit while deselecting all others.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="append">if set to <c>true</c> [append].</param>
        public void ToggleSelected(IUnitFacade unit, bool append)
        {
            //So this works differently depending on the append modifier. If append then the unit is toggled, if not the unit is not toggled but always selected while all others aren't.
            if (!append || _selected == _emptyGroup)
            {
                DeselectAll();

                unit.isSelected = true;
                _selected = GroupingManager.CreateGrouping(unit);
                return;
            }

            unit.isSelected = !unit.isSelected;

            if (unit.isSelected)
            {
                _selected.Add(unit);
            }
            else
            {
                _selected.Remove(unit);
            }

            PostUnitsSelectedMessage(_selected);
        }

        private void PostUnitsSelectedMessage(IGrouping<IUnitFacade> units)
        {
            GameServices.messageBus.Post<UnitsSelectedMessage>(new UnitsSelectedMessage(units));
        }

        /// <summary>
        /// Deselects all currently selected units.
        /// </summary>
        public void DeselectAll()
        {
            _selected.All().Apply(u => u.isSelected = false);
            _selected = _emptyGroup;
        }

        /// <summary>
        /// Assigns the currently selected units to a group.
        /// </summary>
        /// <param name="groupIndex">Index of the group.</param>
        public void AssignGroup(int groupIndex)
        {
            IGrouping<IUnitFacade> group;
            if (_groups.TryGetValue(groupIndex, out group))
            {
                for (int i = 0; i < group.groupCount; i++)
                {
                    group[i].keepAlive = false;
                }
            }

            _groups[groupIndex] = _selected;
            for (int i = 0; i < _selected.groupCount; i++)
            {
                _selected[i].keepAlive = true;
            }
        }

        /// <summary>
        /// Selects a group.
        /// </summary>
        /// <param name="groupIndex">Index of the group.</param>
        public void SelectGroup(int groupIndex)
        {
            IGrouping<IUnitFacade> group;
            if (_groups.TryGetValue(groupIndex, out group))
            {
                _selected.All().Apply(u => u.isSelected = false);
                _selected = group;
                _selected.All().Apply(u => u.isSelected = true);

                PostUnitsSelectedMessage(_selected);
            }
        }

        private class EmptyGrouping : IGrouping<IUnitFacade>
        {
            public int groupCount
            {
                get { return 0; }
            }

            public int memberCount
            {
                get { return 0; }
            }

            public TransientGroup<IUnitFacade> this[int index]
            {
                get { return null; }
            }

            public void Add(IUnitFacade member)
            {
                throw new InvalidOperationException("Cannot add members to this grouping, it is read-only.");
            }

            public void Remove(IUnitFacade member)
            {
                throw new InvalidOperationException("Cannot remove members from this grouping, it is read-only.");
            }
        }
    }
}
