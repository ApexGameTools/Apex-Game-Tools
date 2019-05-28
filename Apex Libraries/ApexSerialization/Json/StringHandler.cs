/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Json
{
    using System.Text;

    internal static class StringHandler
    {
        internal static void EscapeString(string s, StringBuilder b)
        {
            b.EnsureCapacity(s.Length);

            int pendingStart = 0;
            string escaped = null;
            char[] unicodeBuffer = null;
            bool doEscape = false;

            int length = s.Length;
            for (int i = 0; i < length; i++)
            {
                var c = s[i];
                if (c < ' ')
                {
                    switch (c)
                    {
                        case '\t':
                        {
                            escaped = @"\t";
                            break;
                        }

                        case '\n':
                        {
                            escaped = @"\n";
                            break;
                        }

                        case '\r':
                        {
                            escaped = @"\r";
                            break;
                        }

                        case '\f':
                        {
                            escaped = @"\f";
                            break;
                        }

                        case '\b':
                        {
                            escaped = @"\b";
                            break;
                        }

                        case '\0':
                        {
                            escaped = @"\0";
                            break;
                        }

                        default:
                        {
                            doEscape = true;
                            break;
                        }
                    }
                }
                else if (c > 126)
                {
                    doEscape = true;
                }
                else
                {
                    switch (c)
                    {
                        case '\\':
                        {
                            escaped = @"\\";
                            break;
                        }

                        case '\"':
                        {
                            escaped = "\\\"";
                            break;
                        }
                    }
                }

                if (escaped != null || doEscape)
                {
                    //Append all chars up till now
                    if (i > pendingStart)
                    {
                        b.Append(s, pendingStart, i - pendingStart);
                    }

                    pendingStart = i + 1;
                    if (doEscape)
                    {
                        doEscape = false;
                        if (unicodeBuffer == null)
                        {
                            unicodeBuffer = new char[6];
                        }

                        ToCharAsUnicode(c, unicodeBuffer);
                        b.Append(unicodeBuffer);
                    }
                    else
                    {
                        b.Append(escaped);
                        escaped = null;
                    }
                }
            } /* end for each char */

            //Append any remaining chars
            if (pendingStart < length)
            {
                b.Append(s, pendingStart, length - pendingStart);
            }
        }

        //Respectfully borrowed from Json.Net by James Newton-King
        private static void ToCharAsUnicode(char c, char[] buffer)
        {
            buffer[0] = '\\';
            buffer[1] = 'u';
            buffer[2] = IntToHex((c >> 12) & '\x000f');
            buffer[3] = IntToHex((c >> 8) & '\x000f');
            buffer[4] = IntToHex((c >> 4) & '\x000f');
            buffer[5] = IntToHex(c & '\x000f');
        }

        private static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 48);
            }

            return (char)((n - 10) + 97);
        }
    }
}
