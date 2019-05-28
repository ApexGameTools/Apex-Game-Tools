/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Json
{
    internal struct StagedToJson
    {
        private IJsonWriter _json;

        internal StagedToJson(bool pretty)
        {
            if (pretty)
            {
                _json = new JsonPrettyWriter();
            }
            else
            {
                _json = new JsonCompactWriter();
            }
        }

        internal string Serialize(StageElement element)
        {
            WriteElement(element);
            return _json.ToString();
        }

        private void WriteElement(StageElement element)
        {
            bool separate = false;
            _json.WriteElementStart();

            foreach (var a in element.Attributes())
            {
                if (separate)
                {
                    _json.WriteSeparator();
                }
                else
                {
                    separate = true;
                }

                _json.WriteAttributeLabel(a);
                _json.WriteValue(a);
            }

            foreach (var item in element.Items())
            {
                if (separate)
                {
                    _json.WriteSeparator();
                }
                else
                {
                    separate = true;
                }

                _json.WriteLabel(item);

                if (item is StageValue)
                {
                    _json.WriteValue((StageValue)item);
                }
                else if (item is StageElement)
                {
                    WriteElement((StageElement)item);
                }
                else if (item is StageList)
                {
                    WriteList((StageList)item);
                }
                else if (item is StageNull)
                {
                    _json.WriteNull((StageNull)item);
                }
            }

            _json.WriteElementEnd();
        }

        private void WriteList(StageList list)
        {
            _json.WriteListStart();

            bool separate = false;
            foreach (var item in list.Items())
            {
                if (separate)
                {
                    _json.WriteSeparator();
                }
                else
                {
                    separate = true;
                }

                if (item is StageValue)
                {
                    _json.WriteValue((StageValue)item);
                }
                else if (item is StageElement)
                {
                    WriteElement((StageElement)item);
                }
                else if (item is StageList)
                {
                    WriteList((StageList)item);
                }
                else if (item is StageNull)
                {
                    _json.WriteNull((StageNull)item);
                }
            }

            _json.WriteListEnd();
        }
    }
}
