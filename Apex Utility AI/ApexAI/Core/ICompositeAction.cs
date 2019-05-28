/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    /// <summary>
    /// Interface for composites. Used exclusively to differ a composite from other connectors and required for visualization.
    /// </summary>
    /// <seealso cref="Apex.AI.IConnectorAction" />
    public interface ICompositeAction : IConnectorAction
    {
        bool isConnector { get; }
    }
}
