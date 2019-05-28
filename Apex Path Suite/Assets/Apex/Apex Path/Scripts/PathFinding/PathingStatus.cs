/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    /// <summary>
    /// The status of the path request
    /// </summary>
    /// <remarks>
    /// Running: This will never be returned, it is used internally during the processing of a path request.<br />
    /// Failed: This is an error condition that should not happen, and if it does it represents an unrecoverable error in the path finder, i.e.a bug.<br />
    /// 
    /// Decayed: If a path request has been decayed. If an agent issues a path request while it still has a pending request not yet processed by the path finder, the pending request will be decayed and skipped by the path finder.<br />
    /// It will still produce a result which has the status of Decayed.<br />
    /// <br />
    /// StartOutsideGrid: If the agent is not on a grid but the requested destination is, this is returned.<br />
    /// EndOutsideGrid: If the agent is on a grid but the destination is not, this is returned.<br />
    /// <br />
    /// DestinationBlocked: If the destination is blocked by an obstacle, or if the cell is too close to an obstacle for the requesting unit to fit (clearance).<br />
    /// NoRouteExists: If no route exists from the agent to the destination, e.g.the destination is behind a wall through which there is no opening.This will also be returned if the unit itself is standing on a blocked area at the time of the request and no cell is found within ‘Max escape cell distance…’.<br />
    /// <br />
    /// <br />
    /// Complete: The path request succeeded with no errors.<br />
    /// CompletePartial: The path request was partially completed, meaning the path was resolved as far as possible towards the intended goal.<br />
    ///     This happens only for path requests with Via Points.If a path was found part of the way but not all the way to the destination.<br />
    ///     For example say you request a path from A to D via B and C, if a path was found from A to C, then this will be the status of the result.<br />
    ///     The result will also have an innerResult detailing the reason the full path was not found plus the pending waypoints (D). The result will contain the path from A to C.<br />
    /// </remarks>
    public enum PathingStatus
    {
        /// <summary>
        /// The request is currently running
        /// </summary>
        Running,

        /// <summary>
        /// The request failed
        /// </summary>
        Failed,

        /// <summary>
        /// The request decayed
        /// </summary>
        Decayed,

        /// <summary>
        /// The start node is outside grid
        /// </summary>
        StartOutsideGrid,

        /// <summary>
        /// The end node is outside grid
        /// </summary>
        EndOutsideGrid,

        /// <summary>
        /// The no route exists
        /// </summary>
        NoRouteExists,

        /// <summary>
        /// The destination is blocked
        /// </summary>
        DestinationBlocked,

        /// <summary>
        /// The request completed successfully
        /// </summary>
        Complete,

        /// <summary>
        /// The request was partially completed, meaning the path was resolved as far as possible towards the intended goal.
        /// </summary>
        CompletePartial
    }
}
