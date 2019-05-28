/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Json
{
    internal interface IJsonWriter
    {
        void WriteLabel(StageItem l);

        void WriteAttributeLabel(StageAttribute a);

        void WriteElementStart();

        void WriteElementEnd();

        void WriteValue(StageValue v);

        void WriteNull(StageNull n);

        void WriteListStart();

        void WriteListEnd();

        void WriteSeparator();
    }
}
