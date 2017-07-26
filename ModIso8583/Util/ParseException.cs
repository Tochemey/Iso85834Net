using System;

namespace Iso85834Net.Util
{
    public class ParseException : FormatException
    {
        public ParseException(string message) : base(message)
        {
        }
    }
}