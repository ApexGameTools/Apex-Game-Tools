/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor
{
    using System.Reflection;
    using UnityEngine;

    internal sealed class PngResource
    {
        internal PngResource(string name, int width, int height)
            : this(name, width, height, SkinMode.None)
        {
        }

        internal PngResource(string name, int width, int height, SkinMode skinning)
        {
            this.name = name;
            this.width = width;
            this.height = height;
            this.skinning = skinning;

            this.key = string.Format("{0}_{1}_{2}", name, width, height);
        }

        internal int width { get; private set; }

        internal int height { get; private set; }

        internal string name { get; private set; }

        internal string key { get; private set; }

        internal SkinMode skinning { get; private set; }

        internal Texture2D texture
        {
            get
            {
                return ResourceManager.LoadPngResource(this);
            }
        }
    }
}
