using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxEssCloudPoller
{
    public class IntervalTime : IEqualityComparer<IntervalTime>
    {
        private int _intervalInMinutes;

        public DateTime Raw { get; set; }
        public DateTime Rounded { get; set; }

        public IntervalTime(DateTime datetime, int intervalInMinutes)
        {
            //example format from the foxesscloud: 2023-06-24 09:01:24 CEST+0200
            Raw = datetime;
            Rounded = RoundTime(Raw, intervalInMinutes);
            _intervalInMinutes = intervalInMinutes;
        }

        public override bool Equals(object? obj)
        {
            var other = obj as IntervalTime;
            if (other == null) return false;
            return Equals(this, other);
        }

        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        public override string ToString()
        {
            return $"{Raw} -> {Rounded}";
        }

        public bool Equals(IntervalTime? x, IntervalTime? y)
        {
            return x.Rounded == y.Rounded;
        }

        public int GetHashCode([DisallowNull] IntervalTime obj)
        {
            return obj.Rounded.GetHashCode();
        }

        private DateTime RoundTime(DateTime dateTime, int intervalInMinutes) 
        {
            int minute = dateTime.Minute;
            int second = dateTime.Second;
            int totalSeconds = minute * 60 + second;
            int secondsRemainder = totalSeconds % (intervalInMinutes * 60);

            if (secondsRemainder < (intervalInMinutes * 60) / 2)
            {
                return dateTime.AddSeconds(-secondsRemainder);
            }
            else
            {
                return dateTime.AddSeconds((intervalInMinutes * 60) - secondsRemainder);
            }
        }

    }
}
