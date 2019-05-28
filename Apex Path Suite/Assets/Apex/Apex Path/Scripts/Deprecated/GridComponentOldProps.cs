/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.WorldGeometry
{
    using System;
    using System.Collections.Generic;
    using Apex.DataStructures;
    using Apex.Messages;
    using Apex.Services;
    using Apex.Utilities;
    using UnityEngine;

    public partial class GridComponent
    {
        [HideInInspector]
        public float heightGranularity = 0.1f;

        [HideInInspector]
        public float maxWalkableSlopeAngle = 30.0f;

        [HideInInspector]
        public float maxScaleHeight = 0.5f;
    }
}
