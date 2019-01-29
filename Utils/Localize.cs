using System.Globalization;
using System.Threading;

namespace VSTSBot.Utils
{
    public static class Localize
    {
        public static void SetAmbientThreadCulture(string locale)
        {
            if (!string.IsNullOrWhiteSpace(locale))
            {
                CultureInfo found = null;

                try
                {
                    found = CultureInfo.GetCultureInfo(locale);
                }
                catch (CultureNotFoundException)
                {

                }

                if (found != null)
                {
                    Thread.CurrentThread.CurrentCulture = found;
                    Thread.CurrentThread.CurrentUICulture = found;
                }
            }
        }
    }
}