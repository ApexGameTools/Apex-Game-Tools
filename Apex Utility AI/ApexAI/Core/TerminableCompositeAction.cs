/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    /// <summary>
    /// An AI action that is comprised of one or more other actions some or all of which can implement <see cref="IRequireTermination"/>
    /// </summary>
    /// <seealso cref="Apex.AI.CompositeAction" />
    /// <seealso cref="Apex.AI.IRequireTermination" />
    [FriendlyName("Terminable Composite Action", "An action comprised of one or more child actions, which are executed in order. These children can implement IRequireTermination.")]
    public class TerminableCompositeAction : CompositeAction, IRequireTermination
    {
        public void Terminate(IAIContext context)
        {
            int actionsCount = _actions.Count;
            for (int i = 0; i < actionsCount; i++)
            {
                var ta = _actions[i] as IRequireTermination;
                if (ta != null)
                {
                    ta.Terminate(context);
                }
            }
        }
    }
}
