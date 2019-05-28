/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    public interface IConnectorAction : IAction
    {
        IAction Select(IAIContext context);
    }
}
