using System;

namespace VSTSBot.Extensions
{
    public static class ValidatorExtensions
    {
        public static void ThrowIfNull(this object o, string paramName)
        {
            if (o == null)
                throw new ArgumentNullException(paramName);
        }

        public static void ThrowIfNullOrWhiteSpace(this string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(paramName);
            }
        }

    }
}