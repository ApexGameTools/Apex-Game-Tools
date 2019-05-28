namespace Apex.Units
{
    using System;
    using System.Collections.Generic;
    using Apex.DataStructures;
    using Apex.PathFinding;
    using Apex.Steering;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using Messages;
    using UnityEngine;

    /// <summary>
    /// Base class for temporary unit groups.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract partial class TransientGroup<T> : IMovable, IMovingObject, ISortable<T> where T : IGroupable<T>
    {
        private IndexableSet<T> _members;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientGroup{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public TransientGroup(int capacity)
        {
            _members = new IndexableSet<T>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientGroup{T}"/> class.
        /// </summary>
        /// <param name="members">The members.</param>
        public TransientGroup(T[] members)
        {
            Ensure.ArgumentNotNull(members, "members");

            _members = new IndexableSet<T>(members);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientGroup{T}"/> class.
        /// </summary>
        /// <param name="members">The members.</param>
        public TransientGroup(IEnumerable<T> members)
        {
            Ensure.ArgumentNotNull(members, "members");

            _members = new IndexableSet<T>(members);
        }

        /// <summary>
        /// Gets the center of gravity, i.e. the group center.
        /// </summary>
        /// <value>
        /// The center of gravity.
        /// </value>
        public virtual Vector3 centerOfGravity
        {
            get { return GetGroupCenterOfGravity(); }
        }

        /// <summary>
        /// Gets the model unit, i.e. a unit that represents the group.
        /// </summary>
        /// <value>
        /// The model unit.
        /// </value>
        public virtual T modelUnit
        {
            get { return _members.count > 0 ? _members[0] : default(T); }
        }

        /// <summary>
        /// Gets the member count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int count
        {
            get { return _members.count; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this group has arrived.
        /// </summary>
        /// <value>
        /// <c>true</c> if this group has arrived; otherwise, <c>false</c>.
        /// </value>
        public bool hasArrived
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the current path.
        /// </summary>
        /// <value>
        /// The current path.
        /// </value>
        /// <exception cref="System.NotSupportedException"></exception>
        public virtual Path currentPath
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the current way points.
        /// </summary>
        /// <value>
        /// The current way points.
        /// </value>
        /// <exception cref="System.NotSupportedException"></exception>
        public virtual IIterable<Vector3> currentWaypoints
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the final destination, which is either the last point in the <see cref="currentPath" /> or the last of the <see cref="currentWaypoints" /> if there are any.
        /// </summary>
        /// <value>
        /// The final destination.
        /// </value>
        /// <exception cref="System.NotSupportedException"></exception>
        public virtual Vector3? finalDestination
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the latest arrival event.
        /// </summary>
        /// <value>
        /// The latest arrival event.
        /// </value>
        /// <exception cref="System.NotSupportedException"></exception>
        public UnitNavigationEventMessage.Event lastNavigationEvent
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the position of the next node along the path currently being moved towards.
        /// </summary>
        /// <value>
        /// The next node position.
        /// </value>
        /// <exception cref="System.NotSupportedException"></exception>
        public virtual Vector3? nextNodePosition
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets a value indicating whether this object is grounded, i.e. not falling or otherwise raised above its natural base position.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is grounded; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.NotSupportedException"></exception>
        public virtual bool isGrounded
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the velocity of the object. This represents the movement force applied to the object. Also see <see cref="actualVelocity" />.
        /// </summary>
        /// <value>
        /// The velocity.
        /// </value>
        /// <exception cref="System.NotSupportedException"></exception>
        public virtual Vector3 velocity
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the actual velocity of the object. This may differ from <see cref="velocity" /> in certain scenarios, e.g. during collisions, if being moved by other means etc.
        /// </summary>
        /// <value>
        /// The actual velocity.
        /// </value>
        /// <exception cref="System.NotSupportedException"></exception>
        public virtual Vector3 actualVelocity
        {
            get { throw new NotSupportedException(); }
        }

        internal bool keepAlive
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the member at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The member</returns>
        public T this[int index]
        {
            get { return _members[index]; }
        }

        /// <summary>
        /// Adds the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        public virtual void Add(T member)
        {
            _members.Add(member);
        }

        /// <summary>
        /// Removes the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        public virtual void Remove(T member)
        {
            _members.Remove(member);
            if (object.ReferenceEquals(member.transientGroup, this))
            {
                member.transientGroup = null;
            }
        }

        /// <summary>
        /// Sorts this instance using the default comparer of its members.
        /// </summary>
        public virtual void Sort()
        {
            _members.Sort();
        }

        /// <summary>
        /// Sorts this instance using the specified comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public virtual void Sort(IComparer<T> comparer)
        {
            _members.Sort(comparer);
        }

        /// <summary>
        /// Sorts a subset of this instance using the default comparer of its members.
        /// </summary>
        /// <param name="index">The start index.</param>
        /// <param name="length">The length.</param>
        public virtual void Sort(int index, int length)
        {
            _members.Sort(index, length);
        }

        /// <summary>
        /// Sorts a subset of this instance using the specified comparer.
        /// </summary>
        /// <param name="index">The start index.</param>
        /// <param name="length">The length.</param>
        /// <param name="comparer">The comparer.</param>
        public virtual void Sort(int index, int length, IComparer<T> comparer)
        {
            _members.Sort(index, length, comparer);
        }

        /// <summary>
        /// Dissolves this group.
        /// </summary>
        public virtual void Dissolve()
        {
            foreach (var member in _members)
            {
                if (object.ReferenceEquals(member.transientGroup, this))
                {
                    member.transientGroup = null;
                }
            }

            _members.Clear();
        }

        /// <summary>
        /// Gets the group center of gravity.
        /// </summary>
        /// <returns></returns>
        public virtual Vector3 GetGroupCenterOfGravity()
        {
            Vector3 cog = Vector3.zero;

            int membersCount = _members.count;
            if (membersCount == 0)
            {
                return cog;
            }

            for (int i = 0; i < membersCount; i++)
            {
                var member = _members[i] as IPositioned;
                if (member != null)
                {
                    cog += member.position;
                }
            }

            return cog / membersCount;
        }

        /// <summary>
        /// Asks the object to move to the specified position
        /// </summary>
        /// <param name="position">The position to move to.</param>
        /// <param name="append">if set to <c>true</c> the destination is added as a way point.</param>
        public void MoveTo(Vector3 position, bool append)
        {
            PrepareForAction();
            MoveToInternal(position, append);
        }

        /// <summary>
        /// Asks the object to move along the specified path. Replanning is done by the path finder.
        /// </summary>
        /// <param name="path">The path.</param>
        public virtual void MoveAlong(Path path)
        {
            MoveAlong(path, null);
        }

        /// <summary>
        /// Asks the object to move along the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="onReplan">The callback to call when replanning is needed.</param>
        public void MoveAlong(Path path, ReplanCallback onReplan)
        {
            PrepareForAction();
            MoveAlongInternal(path, onReplan);
        }

        /// <summary>
        /// Sets the preferred speed.
        /// </summary>
        /// <param name="speed">The speed.</param>
        public virtual void SetPreferredSpeed(float speed)
        {
            /* Default NOOP */
        }

        /// <summary>
        /// Waits the specified seconds before continuing the move.
        /// </summary>
        /// <param name="seconds">The seconds to wait or null to wait until explicitly <see cref="Resume" />d.</param>
        public abstract void Wait(float? seconds);

        /// <summary>
        /// Resumes movements after a <see cref="Wait" />.
        /// </summary>
        public abstract void Resume();

        /// <summary>
        /// Stops the object's movement.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Enables the movement orders following a call to <see cref="DisableMovementOrders" />.
        /// </summary>
        public abstract void EnableMovementOrders();

        /// <summary>
        /// Disables movement orders, i.e. calls to <see cref="MoveTo" /> will be ignored until <see cref="EnableMovementOrders" /> is called.
        /// </summary>
        public abstract void DisableMovementOrders();

        /// <summary>
        /// Internal implementation of <see cref="MoveTo"/>
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="append">if set to <c>true</c> the destination is added as a way point.</param>
        protected abstract void MoveToInternal(Vector3 position, bool append);

        /// <summary>
        /// Internal implementation of <see cref="MoveAlongInternal"/>
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="onReplan">The callback to call when replanning is needed.</param>
        protected abstract void MoveAlongInternal(Path path, ReplanCallback onReplan);

        /// <summary>
        /// Prepares this group for action, making all its members recognize it as their active group.
        /// </summary>
        protected virtual void PrepareForAction()
        {
            for (int i = 0; i < _members.count; i++)
            {
                var member = _members[i];
                var group = member.transientGroup;
                if (group == null || !object.ReferenceEquals(group, this))
                {
                    if (group != null && !group.keepAlive)
                    {
                        group.Remove(member);
                    }

                    member.transientGroup = this;
                }
            }
        }
    }
}