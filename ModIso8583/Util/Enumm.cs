using System;

namespace Iso85834Net.Util
{
    public static class Enumm
    {
        public static T? Parse<T>(string name) where T : struct
        {
            return (T) Enum.Parse(typeof(T),
                name,
                true);
        }
    }
}