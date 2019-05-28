/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Json
{
    using System;
    using System.Globalization;

    /// <summary>
    /// A simple recursive json reader. It will not handle any type of syntax or semantic errors.
    /// </summary>
    internal sealed class JsonParser : IJsonParser
    {
        private string _s;
        private int _idx;
        private int _length;
        private int _valStart;
        private int _valEnd;
        private StringBuffer _b;
        private char[] _hexBuffer;
        private StageContainer _curRoot;

        internal JsonParser()
        {
            _b = new StringBuffer(1024);
            _hexBuffer = new char[4];
        }

        public StageElement Parse(string json)
        {
            _s = json;
            _b.position = 0;
            _curRoot = null;
            _length = json != null ? json.Length : 0;

            for (_idx = 0; _idx < _length; _idx++)
            {
                var c = _s[_idx];
                switch (c)
                {
                    case '{':
                    {
                        _idx++;
                        ParseElement(null);
                        break;
                    }
                }
            }

            return _curRoot as StageElement;
        }

        private void ParseElement(string name)
        {
            var el = new StageElement(name);
            if (_curRoot != null)
            {
                _curRoot.Add(el);
            }

            _curRoot = el;

            for (; _idx < _length; _idx++)
            {
                var c = _s[_idx];
                switch (c)
                {
                    case ',':
                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    {
                        /* Skip white space and item separator */
                        break;
                    }

                    case '}':
                    {
                        _curRoot = _curRoot.parent ?? _curRoot;
                        return;
                    }

                    default:
                    {
                        ParseItem();
                        break;
                    }
                }
            }
        }

        private void ParseList(string name)
        {
            var l = new StageList(name);
            _curRoot.Add(l);

            _curRoot = l;

            for (; _idx < _length; _idx++)
            {
                var c = _s[_idx];
                switch (c)
                {
                    case ',':
                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    {
                        /* Skip white space and item separator */
                        break;
                    }

                    case ']':
                    {
                        _curRoot = _curRoot.parent ?? _curRoot;
                        return;
                    }

                    default:
                    {
                        ParseItemType(null, false);
                        break;
                    }
                }
            }
        }

        private void ParseItem()
        {
            bool isAttribute = false;
            for (; _idx < _length; _idx++)
            {
                var c = _s[_idx];
                switch (c)
                {
                    case '"':
                    {
                        _idx++;
                        if (_s[_idx] == '@')
                        {
                            isAttribute = true;
                            _idx++;
                        }

                        ParseString();
                        break;
                    }

                    case ':':
                    {
                        _idx++;
                        ParseItemType(_b.Flush(), isAttribute);
                        return;
                    }

                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    {
                        /* Skip white space */
                        break;
                    }
                }
            }
        }

        private void ParseItemType(string name, bool isAttribute)
        {
            for (; _idx < _length; _idx++)
            {
                var c = _s[_idx];
                switch (c)
                {
                    case '{':
                    {
                        _idx++;
                        ParseElement(name);
                        return;
                    }

                    case '[':
                    {
                        _idx++;
                        ParseList(name);
                        return;
                    }

                    case '"':
                    {
                        _idx++;
                        ParseString();
                        var item = isAttribute ? new StageAttribute(name, _b.Flush(), true) : new StageValue(name, _b.Flush(), true);
                        _curRoot.Add(item);
                        return;
                    }

                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    {
                        /* Skip white space */
                        break;
                    }

                    default:
                    {
                        ParseValue(name, isAttribute);
                        return;
                    }
                }
            }
        }

        private void ParseValue(string name, bool isAttribute)
        {
            _valStart = _idx;
            _valEnd = -1;
            for (; _idx < _length; _idx++)
            {
                var c = _s[_idx];
                switch (c)
                {
                    case ',':
                    case ']':
                    case '}':
                    {
                        if (_valEnd >= 0)
                        {
                            _b.Append(_s, _valStart, (_valEnd - _valStart) + 1);
                        }

                        var item = isAttribute ? new StageAttribute(name, _b.Flush(), false) : new StageValue(name, _b.Flush(), false);
                        _curRoot.Add(item);
                        _idx--;
                        return;
                    }

                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    {
                        /* Skip white space */
                        if (_valEnd == -1)
                        {
                            _valStart++;
                        }

                        break;
                    }

                    default:
                    {
                        _valEnd = _idx;
                        break;
                    }
                }
            }
        }

        private void ParseString()
        {
            _valStart = _idx;
            _valEnd = -1;
            for (; _idx < _length; _idx++)
            {
                var c = _s[_idx];
                switch (c)
                {
                    case '"':
                    {
                        if (_valEnd >= 0)
                        {
                            _b.Append(_s, _valStart, (_valEnd - _valStart) + 1);
                        }

                        return;
                    }

                    case '\\':
                    {
                        if (_valEnd >= 0)
                        {
                            _b.Append(_s, _valStart, (_valEnd - _valStart) + 1);
                            _valEnd = -1;
                        }

                        _idx++;
                        UnescapeChar();
                        _valStart = _idx + 1;
                        break;
                    }

                    default:
                    {
                        _valEnd = _idx;
                        break;
                    }
                }
            }
        }

        private void UnescapeChar()
        {
            var c = _s[_idx];
            switch (c)
            {
                case '\\':
                case '"':
                {
                    _b.Append(c);
                    break;
                }

                case 't':
                {
                    _b.Append('\t');
                    break;
                }

                case 'n':
                {
                    _b.Append('\n');
                    break;
                }

                case 'r':
                {
                    _b.Append('\r');
                    break;
                }

                case 'f':
                {
                    _b.Append('\f');
                    break;
                }

                case 'b':
                {
                    _b.Append('\b');
                    break;
                }

                case '0':
                {
                    _b.Append('\0');
                    break;
                }

                case 'u':
                {
                    _s.CopyTo(_idx + 1, _hexBuffer, 0, 4);
                    string hexValues = new string(_hexBuffer);
                    char hexChar = Convert.ToChar(int.Parse(hexValues, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo));
                    _b.Append(hexChar);
                    _idx += 4;
                    break;
                }
            }
        }
    }
}
