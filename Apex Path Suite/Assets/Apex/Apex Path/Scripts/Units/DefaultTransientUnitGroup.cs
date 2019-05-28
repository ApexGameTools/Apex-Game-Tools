/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Units
{
    using System.Collections.Generic;
    using Apex.DataStructures;
    using Apex.PathFinding;
    using UnityEngine;

    /// <summary>
    /// The default group type for basic steering. This group will basically delegate any command onto its members.
    /// </summary>
    public class DefaultTransientUnitGroup : TransientGroup<IUnitFacade>, IGrouping<IUnitFacade>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTransientUnitGroup"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public DefaultTransientUnitGroup(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTransientUnitGroup"/> class.
        /// </summary>
        /// <param name="members">The members.</param>
        public DefaultTransientUnitGroup(IUnitFacade[] members)
            : base(members)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTransientUnitGroup"/> class.
        /// </summary>
        /// <param name="members">The members.</param>
        public DefaultTransientUnitGroup(IEnumerable<IUnitFacade> members)
            : base(members)
        {
        }

        /// <summary>
        /// Gets the velocity of the object. This represents the movement force applied to the object. Also see <see cref="actualVelocity" />.
        /// </summary>
        /// <value>
        /// The velocity.
        /// </value>
        public override Vector3 velocity
        {
            get
            {
                var mu = this.modelUnit;
                return mu != null ? mu.velocity : Vector3.zero;
            }
        }

        /// <summary>
        /// Gets the actual velocity of the object. This may differ from <see cref="velocity" /> in certain scenarios, e.g. during collisions, if being moved by other means etc.
        /// </summary>
        /// <value>
        /// The actual velocity.
        /// </value>
        public override Vector3 actualVelocity
        {
            get
            {
                var mu = this.modelUnit;
                return mu != null ? mu.actualVelocity : Vector3.zero;
            }
        }

        /// <summary>
        /// Gets the current path.
        /// </summary>
        /// <value>
        /// The current path.
        /// </value>
        public override Path currentPath
        {
            get
            {
                var mu = this.modelUnit;
                return mu != null ? mu.currentPath : null;
            }
        }

        /// <summary>
        /// Gets the current way points.
        /// </summary>
        /// <value>
        /// The current way points.
        /// </value>
        public override IIterable<Vector3> currentWaypoints
        {
            get
            {
                var mu = this.modelUnit;
                return mu != null ? mu.currentWaypoints : null;
            }
        }

        /// <summary>
        /// Gets the final destination, which is either the last point in the <see cref="currentPath" /> or the last of the <see cref="currentWaypoints" /> if there are any.
        /// </summary>
        /// <value>
        /// The final destination.
        /// </value>
        public override Vector3? finalDestination
        {
            get
            {
                var mu = this.modelUnit;
                return mu != null ? mu.finalDestination : null;
            }
        }

        /// <summary>
        /// Gets the position of the next node along the path currently being moved towards.
        /// </summary>
        /// <value>
        /// The next node position.
        /// </value>
        public override Vector3? nextNodePosition
        {
            get
            {
                var mu = this.modelUnit;
                return mu != null ? mu.nextNodePosition : null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this object is grounded, i.e. not falling or otherwise raised above its natural base position.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is grounded; otherwise, <c>false</c>.
        /// </value>
        public override bool isGrounded
        {
            get { return modelUnit.isGrounded; }
        }

        int IGrouping<IUnitFacade>.groupCount
        {
            get { return 1; }
        }

        int IGrouping<IUnitFacade>.memberCount
        {
            get { return this.count; }
        }

        TransientGroup<IUnitFacade> IGrouping<IUnitFacade>.this[int index]
        {
            get { return this; }
        }

        /// <summary>
        /// Waits the specified seconds before continuing the move.
        /// </summary>
        /// <param name="seconds">The seconds to wait or null to wait until explicitly <see cref="Resume" />d.</param>
        public override void Wait(float? seconds)
        {
            for (int i = 0; i < this.count; i++)
            {
                this[i].Wait(seconds);
            }
        }

        /// <summary>
        /// Resumes movements after a <see cref="Wait" />.
        /// </summary>
        public override void Resume()
        {
            for (int i = 0; i < this.count; i++)
            {
                this[i].Resume();
            }
        }

        /// <summary>
        /// Stops the object's movement.
        /// </summary>
        public override void Stop()
        {
            for (int i = 0; i < this.count; i++)
            {
                this[i].Stop();
            }
        }

        /// <summary>
        /// Enables the movement orders following a call to <see cref="DisableMovementOrders" />.
        /// </summary>
        public override void EnableMovementOrders()
        {
            for (int i = 0; i < this.count; i++)
            {
                this[i].EnableMovementOrders();
            }
        }

        /// <summary>
        /// Disables movement orders, i.e. calls to <see cref="Apex.Steering.IMovable.MoveTo" /> will be ignored until <see cref="EnableMovementOrders" /> is called.
        /// </summary>
        public override void DisableMovementOrders()
        {
            for (int i = 0; i < this.count; i++)
            {
                this[i].DisableMovementOrders();
            }
        }

        /// <summary>
        /// Sets the preferred speed.
        /// </summary>
        /// <param name="speed">The speed.</param>
        public override void SetPreferredSpeed(float speed)
        {
            var gcount = this.count;
            for (int i = 0; i < gcount; i++)
            {
                this[i].SetPreferredSpeed(speed);
            }

            var mu = this.modelUnit;
            if (mu != null)
            {
                mu.SetPreferredSpeed(speed);
            }
        }

        void IGrouping<IUnitFacade>.Remove(IUnitFacade member)
        {
            this.Remove(member);
        }

        void IGrouping<IUnitFacade>.Add(IUnitFacade member)
        {
            this.Add(member);
        }

        /// <summary>
        /// Internal implementation of <see cref="Apex.Steering.IMovable.MoveTo" />
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="append">if set to <c>true</c> the destination is added as a way point.</param>
        protected override void MoveToInternal(Vector3 position, bool append)
        {
            for (int i = 0; i < this.count; i++)
            {
                this[i].MoveTo(position, append);
            }
        }

        /// <summary>
        /// Internal implementation of <see cref="Apex.Steering.IMovable.MoveAlong(Path, ReplanCallback)" />
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="onReplan">The callback to call when replanning is needed.</param>
        protected override void MoveAlongInternal(Path path, ReplanCallback onReplan)
        {
            for (int i = 0; i < this.count; i++)
            {
                var clone = path.Clone();
                this[i].MoveAlong(clone, onReplan);
            }
        }
    }
}
