/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Json
{
    using System.Text;

    internal class JsonCompactWriter : IJsonWriter
    {
        private StringBuilder _b = new StringBuilder();

        public void WriteLabel(StageItem l)
        {
            _b.Append('"');
            StringHandler.EscapeString(l.name, _b);
            _b.Append('"');
            _b.Append(':');
        }

        public void WriteAttributeLabel(StageAttribute a)
        {
            _b.Append('"');
            _b.Append('@');
            StringHandler.EscapeString(a.name, _b);
            _b.Append('"');
            _b.Append(':');
        }

        public void WriteElementStart()
        {
            _b.Append('{');
        }

        public void WriteElementEnd()
        {
            _b.Append('}');
        }

        public void WriteListStart()
        {
            _b.Append('[');
        }

        public void WriteListEnd()
        {
            _b.Append(']');
        }

        public void WriteNull(StageNull n)
        {
            _b.Append("null");
        }

        public void WriteValue(StageValue v)
        {
            if (v.isText)
            {
                _b.Append('"');
                StringHandler.EscapeString(v.value, _b);
                _b.Append('"');
            }
            else
            {
                _b.Append(v.value);
            }
        }

        public void WriteSeparator()
        {
            _b.Append(',');
        }

        public override string ToString()
        {
            return _b.ToString();
        }
    }
}
