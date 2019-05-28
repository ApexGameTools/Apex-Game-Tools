/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System.Collections.Generic;

    /// <summary>
    /// Combination interface exposing both <see cref="IIndexable&lt;T&gt;"/> and <see cref="IEnumerable&lt;T&gt;"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IIterable<T> : IEnumerable<T>, IIndexable<T>
    {
    }
}
