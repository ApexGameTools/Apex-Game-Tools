/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI
{
    using System;
    using System.Collections.Generic;
    using Apex.Serialization;

    /// <summary>
    /// Base class for selectors. To create a new type of selector you must inherit from this.
    /// </summary>
    public abstract class Selector : ISelect, ICanClone
    {
        /// <summary>
        /// The ID of the selector
        /// </summary>
        [ApexSerialization(hideInEditor = true)]
        protected Guid _id;

        /// <summary>
        /// The list of qualifiers for this selector
        /// </summary>
        [ApexSerialization(hideInEditor = true)]
        protected List<IQualifier> _qualifiers;

        /// <summary>
        /// The default qualifier of this selector
        /// </summary>
        [ApexSerialization(hideInEditor = true)]
        protected IDefaultQualifier _defaultQualifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="Selector"/> class.
        /// </summary>
        public Selector()
        {
            _id = Guid.NewGuid();
            _qualifiers = new List<IQualifier>();
            _defaultQualifier = new DefaultQualifier();
        }

        /// <summary>
        /// Gets the qualifiers of this selector.
        /// </summary>
        /// <value>
        /// The qualifiers.
        /// </value>
        public List<IQualifier> qualifiers
        {
            get { return _qualifiers; }
        }

        /// <summary>
        /// Gets or sets the default qualifier.
        /// </summary>
        /// <value>
        /// The default qualifier.
        /// </value>
        /// <exception cref="ArgumentNullException">Value cannot be null.;defaultQualifier</exception>
        public IDefaultQualifier defaultQualifier
        {
            get
            {
                return _defaultQualifier;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Value cannot be null.", "defaultQualifier");
                }

                _defaultQualifier = value;
            }
        }

        /// <summary>
        /// Gets the ID of this selector.
        /// </summary>
        /// <value>
        /// The ID.
        /// </value>
        public Guid id
        {
            get { return _id; }
        }

        /// <summary>
        /// Selects the action for execution, given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The action to execute.
        /// </returns>
        public IAction Select(IAIContext context)
        {
            var qualifier = Select(context, _qualifiers, _defaultQualifier);
            if (qualifier != null)
            {
                return qualifier.action;
            }

            return null;
        }

        /// <summary>
        /// Selects the action for execution, given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="qualifiers">The qualifiers from which to find the action.</param>
        /// <param name="defaultQualifier">The default qualifier.</param>
        /// <returns>The qualifier whose action should be chosen.</returns>
        public abstract IQualifier Select(IAIContext context, IList<IQualifier> qualifiers, IDefaultQualifier defaultQualifier);

        /// <summary>
        /// Regenerates the ID.
        /// </summary>
        public void RegenerateId()
        {
            _id = Guid.NewGuid();
        }

        /// <summary>
        /// Clones or transfers settings from the other entity to itself.
        /// </summary>
        /// <param name="other">The other entity.</param>
        public virtual void CloneFrom(object other)
        {
            var source = other as Selector;
            if (source == null)
            {
                return;
            }

            foreach (var q in source.qualifiers)
            {
                _qualifiers.Add(q);
            }

            _id = source.id;
            _defaultQualifier = source._defaultQualifier;
        }
    }
}