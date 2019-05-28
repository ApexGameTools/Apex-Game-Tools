/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    /// <summary>
    /// Marker interface for AI views
    /// </summary>
    internal interface IView
    {
        AIUI parentUI { get; }

        string name { get; set; }

        string description { get; set; }
    }
}
