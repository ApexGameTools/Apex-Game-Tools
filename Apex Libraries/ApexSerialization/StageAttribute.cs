namespace Apex.Serialization
{
    /// <summary>
    /// Staged representation of an attribute.
    /// </summary>
    /// <seealso cref="Apex.Serialization.StageValue" />
    public sealed class StageAttribute : StageValue
    {
        internal StageAttribute(string name, string value, bool isText)
            : base(name, value, isText)
        {
        }
    }
}
