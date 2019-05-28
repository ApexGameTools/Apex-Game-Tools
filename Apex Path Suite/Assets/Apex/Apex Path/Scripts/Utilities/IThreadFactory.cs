/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Utilities
{
    using System.Threading;

    /// <summary>
    /// Interface for thread factories.
    /// </summary>
    public interface IThreadFactory
    {
#if !NETFX_CORE
        /// <summary>
        /// Creates a thread.
        /// </summary>
        /// <param name="name">The name of the thread.</param>
        /// <param name="proc">The procedure to run in the thread.</param>
        /// <returns>The thread</returns>
        Thread CreateThread(string name, ParameterizedThreadStart proc);
#endif
    }
}
