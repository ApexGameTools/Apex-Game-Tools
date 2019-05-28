/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    internal struct XRange
    {
        internal float xMin;
        internal float xMax;
        internal float width;

        internal XRange(float x, float width)
        {
            this.xMin = x;
            this.xMax = x + width;
            this.width = width;
        }

        internal bool Contains(float xpos)
        {
            return (xpos >= this.xMin) && (xpos < this.xMax);
        }
    }
}
