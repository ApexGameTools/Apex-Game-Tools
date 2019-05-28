/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Json
{
    using System.Text;

    internal class JsonPrettyWriter : IJsonWriter
    {
        private StringBuilder _b = new StringBuilder();
        private int _depth = 0;
        private bool _indent;

        public void WriteLabel(StageItem l)
        {
            Indent();
            _b.Append('"');
            StringHandler.EscapeString(l.name, _b);
            _b.Append('"');
            _b.Append(" : ");
        }

        public void WriteAttributeLabel(StageAttribute a)
        {
            Indent();
            _b.Append('"');
            _b.Append('@');
            StringHandler.EscapeString(a.name, _b);
            _b.Append('"');
            _b.Append(" : ");
        }

        public void WriteElementStart()
        {
            Indent();
            _b.Append('{');
            NextLine(1);
        }

        public void WriteElementEnd()
        {
            NextLine(-1);
            Indent();
            _b.Append('}');
        }

        public void WriteListStart()
        {
            _b.Append('[');
            NextLine(1);
        }

        public void WriteListEnd()
        {
            NextLine(-1);
            Indent();
            _b.Append(']');
        }

        public void WriteNull(StageNull n)
        {
            Indent();
            _b.Append("null");
        }

        public void WriteValue(StageValue v)
        {
            Indent();

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
            NextLine(0);
        }

        public override string ToString()
        {
            return _b.ToString();
        }

        private void Indent()
        {
            if (_indent)
            {
                _b.Append(' ', _depth * 2);
                _indent = false;
            }
        }

        private void NextLine(int depthChange)
        {
            _indent = true;
            _depth += depthChange;
            _b.AppendLine();
        }
    }
}
