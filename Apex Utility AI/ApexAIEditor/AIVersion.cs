namespace Apex.AI.Editor
{
    using System;

    /// <summary>
    /// Stores the version in a single int. Obviously this puts some restrictions on the version, each of major, minor etc. can be a max of 255.
    /// </summary>
    internal struct AIVersion
    {
        internal int version;

        public AIVersion(int version)
        {
            this.version = version;
        }

        internal static AIVersion FromVersion(Version v)
        {
            return new AIVersion
            {
                version = (v.Major << 24) + (v.Minor << 16) + (v.Build << 8) + v.Revision
            };
        }

        internal Version ToVersion()
        {
            return new Version(
                version >> 24,
                version & 0x00FF0000,
                version & 0x0000FF00,
                version & 0x000000FF);
        }
    }
}
