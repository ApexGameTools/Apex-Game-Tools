/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Represents a group of steering behaviours
    /// </summary>
    public class SteeringGroup : ISteeringBehaviour
    {
        private readonly SteeringOutput _memberOutput;
        private readonly List<ISteeringBehaviour> _steeringComponents;

        /// <summary>
        /// Initializes a new instance of the <see cref="SteeringGroup"/> class.
        /// </summary>
        /// <param name="priority">The priority of the group.</param>
        public SteeringGroup(int priority)
        {
            this.priority = priority;
            _memberOutput = new SteeringOutput();
            _steeringComponents = new List<ISteeringBehaviour>();
        }

        /// <summary>
        /// Gets the priority of this steering behaviour relative to others. Only behaviours with the highest priority will influence the steering of the unit, provided they have any steering output.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public int priority
        {
            get;
            private set;
        }

        /// <summary>
        /// Adds the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <exception cref="System.ArgumentException">Invalid priority</exception>
        public void Add(ISteeringBehaviour member)
        {
            if (member.priority != this.priority)
            {
                throw new ArgumentException("Invalid priority");
            }

            _steeringComponents.Add(member);
        }

        /// <summary>
        /// Removes the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        public void Remove(ISteeringBehaviour member)
        {
            _steeringComponents.Remove(member);
        }

        /// <summary>
        /// Gets the steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        public void GetSteering(SteeringInput input, SteeringOutput output)
        {
            for (int i = 0; i < _steeringComponents.Count; i++)
            {
                var c = _steeringComponents[i];

                _memberOutput.Clear();
                c.GetSteering(input, _memberOutput);

                output.overrideHeightNavigation |= _memberOutput.overrideHeightNavigation;
                if (_memberOutput.hasOutput)
                {
                    output.desiredAcceleration += _memberOutput.desiredAcceleration;
                    output.verticalForce += _memberOutput.verticalForce;
                    output.maxAllowedSpeed = Mathf.Max(output.maxAllowedSpeed, _memberOutput.maxAllowedSpeed);
                    output.pause |= _memberOutput.pause;
                }
            }
        }

        /// <summary>
        /// Stops this steering behaviour.
        /// </summary>
        public void Stop()
        {
            for (int i = 0; i < _steeringComponents.Count; i++)
            {
                _steeringComponents[i].Stop();
            }
        }
    }
}
