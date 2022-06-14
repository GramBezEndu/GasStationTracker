namespace GasStationTracker.Helpers
{
    using System.Text.RegularExpressions;

    public static class RegexHelper
    {
        public static string AddSpacesToCamelCase(this string text)
        {
            return Regex.Replace(text, "(\\B[A-Z0-9])", " $1");
        }
    }
}
