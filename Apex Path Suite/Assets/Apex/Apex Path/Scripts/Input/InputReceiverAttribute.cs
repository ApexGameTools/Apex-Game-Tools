namespace Apex.Input
{
    using System;

    /// <summary>
    /// This is simply a marker attribute to identify an Input Receiver
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InputReceiverAttribute : Attribute
    {
    }
}
