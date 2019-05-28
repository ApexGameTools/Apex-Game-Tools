/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Editor
{
    using System.Reflection;
    using UnityEngine;

    internal sealed class PngResource
    {
        internal PngResource(string name, int width, int height)
        {
            this.name = name;
            this.width = width;
            this.height = height;

            this.key = string.Format("{0}_{1}_{2}", name, width, height);
        }

        internal int width { get; private set; }

        internal int height { get; private set; }

        internal string name { get; private set; }

        internal string key { get; private set; }

        internal Texture2D texture
        {
            get
            {
                return ResourceManager.LoadPngResource(this);
            }
        }
    }
}
