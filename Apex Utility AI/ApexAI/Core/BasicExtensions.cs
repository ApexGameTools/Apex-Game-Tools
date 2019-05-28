/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// A range of basic extensions.
    /// </summary>
    public static class BasicExtensions
    {      
        /// <summary>
        /// Executes an AI once.
        /// </summary>
        /// <param name="ai">The AI.</param>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if an action was selected and executed; otherwise <c>false</c></returns>
        public static bool ExecuteOnce(this IUtilityAI ai, IAIContext context)
        {
            var action = ai.Select(context);

            bool finalActionFound = false;
            bool actionExecuted = false;

            while (!finalActionFound)
            {
                //While we could treat all connectors the same, most connectors will not have anything to execute, so this way we save the call to Execute.
                var composite = action as ICompositeAction;
                if (composite == null)
                {
                    var connector = action as IConnectorAction;
                    if (connector == null)
                    {
                        finalActionFound = true;
                    }
                    else
                    {
                        action = connector.Select(context);
                    }
                }
                else
                {
                    //For composites that also connect, we execute the child actions before moving on.
                    //So action is executed and then reassigned to the selected action if one exists.
                    action.Execute(context);
                    action = composite.Select(context);
                    finalActionFound = (action == null);
                    actionExecuted = true;
                }
            }

            if (action != null)
            {
                action.Execute(context);
                actionExecuted = true;
            }

            return actionExecuted;
        }

        internal static IEnumerable<IQualifier> AllQualifiers(this Selector source)
        {
            var qualifiers = source.qualifiers;
            var count = qualifiers.Count;
            for (int i = 0; i < count; i++)
            {
                yield return qualifiers[i];
            }

            yield return source.defaultQualifier;
        }

        internal static bool IsConnectedTo(this Selector source, Selector target)
        {
            foreach (var q in source.AllQualifiers())
            {
                var sa = q.action as SelectorAction;
                if (sa == null)
                {
                    var ca = q.action as CompositeAction;
                    if (ca == null)
                    {
                        continue;
                    }

                    sa = ca.connectorAction as SelectorAction;
                    if (sa == null)
                    {
                        continue;
                    }
                }

                if (object.ReferenceEquals(sa.selector, target))
                {
                    return true;
                }

                return IsConnectedTo(sa.selector, target);
            }

            return false;
        }
    }
}
