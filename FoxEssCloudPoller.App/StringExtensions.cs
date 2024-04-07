using System.Text.RegularExpressions;

namespace FoxEssCloudPoller
{
    internal static class StringExtensions
    {
        private static readonly Regex _timeRegex = new Regex(@"^(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2}) (?<hour>\d{2}):(?<minute>\d{2}):(?<second>\d{2}) (?<timezoneAbbreviation>\w+)(?<offsetSign>[-+])(?<offsetHours>\d{2})(?<offsetMinutes>\d{2})$", RegexOptions.Compiled);

        public static DateTime ToDateTime(this string value)
        {
            var match = _timeRegex.Match(value);
            if (match.Success)
            {
                int year = int.Parse(match.Groups["year"].Value);
                int month = int.Parse(match.Groups["month"].Value);
                int day = int.Parse(match.Groups["day"].Value);
                int hour = int.Parse(match.Groups["hour"].Value);
                int minute = int.Parse(match.Groups["minute"].Value);
                int second = int.Parse(match.Groups["second"].Value);
                //int offsetSign = match.Groups["offsetSign"].Value == "+" ? 1 : -1;
                //int offsetHours = int.Parse(match.Groups["offsetHours"].Value);
                //int offsetMinutes = int.Parse(match.Groups["offsetMinutes"].Value);

                return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
            }
            else throw new FormatException("The time string is not in the expected format.");
        }
    }
}