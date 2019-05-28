/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using System;

    /// <summary>
    /// An attribute to decorate AI entity fields and properties to set dependency on another field or property.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class MemberDependencyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberDependencyAttribute"/> class.
        /// </summary>
        /// <param name="dependsOn">The name of the field/property to set a dependency on.</param>
        /// <param name="value">The value that satisfies the dependency.</param>
        /// <param name="match">The mask matching rule.</param>
        public MemberDependencyAttribute(string dependsOn, int value, MaskMatch match)
        {
            this.dependsOn = dependsOn;
            this.value = value;
            this.match = match;
            this.isMask = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberDependencyAttribute"/> class.
        /// </summary>
        /// <param name="dependsOn">The name of the field/property to set a dependency on.</param>
        /// <param name="value">The value that satisfies the dependency.</param>
        /// <param name="compare">The compare operator used to check if the dependency is satisfied.</param>
        public MemberDependencyAttribute(string dependsOn, int value, CompareOperator compare)
        {
            this.dependsOn = dependsOn;
            this.value = value;
            this.compare = compare;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberDependencyAttribute"/> class.
        /// </summary>
        /// <param name="dependsOn">The name of the field/property to set a dependency on.</param>
        /// <param name="value">The value that satisfies the dependency.</param>
        /// <param name="compare">The compare operator used to check if the dependency is satisfied.</param>
        public MemberDependencyAttribute(string dependsOn, float value, CompareOperator compare)
        {
            this.dependsOn = dependsOn;
            this.value = value;
            this.compare = compare;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberDependencyAttribute"/> class.
        /// </summary>
        /// <param name="dependsOn">The name of the field/property to set a dependency on.</param>
        /// <param name="value">The value that satisfies the dependency.</param>
        public MemberDependencyAttribute(string dependsOn, bool value)
        {
            this.dependsOn = dependsOn;
            this.value = value;
            this.compare = CompareOperator.Equals;
        }

        /// <summary>
        /// Gets the name of the field/property to set a dependency on.
        /// </summary>
        /// <value>
        /// The name of the field/property to set a dependency on.
        /// </value>
        public string dependsOn
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the value that satisfies the dependency.
        /// </summary>
        /// <value>
        /// The value that satisfies the dependency.
        /// </value>
        public ValueType value
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the mask matching rule. Only relevant if <see cref="value"/> is an <see cref="Int32"/>
        /// </summary>
        /// <value>
        /// The mask matching rule.
        /// </value>
        public MaskMatch match
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the compare operator used to check if the dependency is satisfied.
        /// </summary>
        /// <value>
        /// The compare operator used to check if the dependency is satisfied.
        /// </value>
        public CompareOperator compare
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the dependency check is a mask check or a basic comparison.
        /// </summary>
        public bool isMask
        {
            get;
            private set;
        }
    }
}
