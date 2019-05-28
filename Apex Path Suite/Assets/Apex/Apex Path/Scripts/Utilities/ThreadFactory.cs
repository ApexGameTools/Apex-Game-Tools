/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Utilities
{
    using System.Threading;

    /// <summary>
    /// Default thread factory.
    /// </summary>
    public class ThreadFactory : IThreadFactory
    {
#if !NETFX_CORE
        /// <summary>
        /// Creates a thread.
        /// </summary>
        /// <param name="name">The name of the thread.</param>
        /// <param name="proc">The procedure to run in the thread.</param>
        /// <returns>
        /// The thread
        /// </returns>
        public Thread CreateThread(string name, ParameterizedThreadStart proc)
        {
            var t = new Thread(proc);
            t.Name = name;

            return t;
        }
#endif
    }
}
