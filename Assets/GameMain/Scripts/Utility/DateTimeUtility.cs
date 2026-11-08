using System;

namespace MergeSword
{
    /// <summary>
    /// 统一使用UTC作为基准，本地时间仅作为外部展示
    /// </summary>
    public static class DateTimeUtility
    {
        private static readonly DateTime UtcEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 日期转 UTC 时间戳（秒）。
        /// </summary>
        public static long ToTimeStamp(this DateTime dateTime)
        {
            var elapsedTime = dateTime - UtcEpoch;
            return (long) elapsedTime.TotalSeconds;
        }

        /// <summary>
        /// UTC 时间戳（秒）转本地 DateTime。
        /// </summary>
        public static DateTime ToLocalDateTime(this long utcTimeStamp)
        {
            return UtcEpoch.AddSeconds(utcTimeStamp).ToLocalTime();
        }

        /// <summary>
        /// UTC 时间戳（秒）转本地日期文本，格式如 "2024-07-15 10:30:00"。
        /// </summary>
        public static string ToLocalDateString(this long utcTimeStamp)
        {
            return UtcEpoch.AddSeconds(utcTimeStamp).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// UTC 时间戳（秒）转 UTC DateTime。
        /// </summary>
        public static DateTime ToUtcDateTime(this long timeStamp)
        {
            return UtcEpoch.AddSeconds(timeStamp);
        }

        public static DateTime TodayBeginning(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
        }

        public static DateTime TodayEnding(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59);
        }
        
        public static string ToHMSDateTimeString(this float seconds)
        {
            return ToDateTimeString(seconds, @"hh\:mm\:ss");
        }
        public static string ToMSDateTimeString(this float seconds)
        {
            return ToDateTimeString(seconds, @"mm\:ss");
        }
        
        public static string ToDateTimeString(this float time,string format)
        {
            var timeSpan = TimeSpan.FromSeconds(time);
            return timeSpan.ToString(format);
        }
        
        /// <summary>
        /// 转为 "Xd hh:mm:ss" 格式的字符串（>=1天显示天数，否则显示 hh:mm:ss）
        /// </summary>
        public static string ToDHMSDateTimeString(this float seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            if (timeSpan.TotalDays >= 1d)
            {
                var days = (int)timeSpan.TotalDays;
                var remaining = timeSpan - TimeSpan.FromDays(days);
                return $"{days}d {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
            }
            else
            {
                return timeSpan.ToString(@"hh\:mm\:ss");
            }
        }
    }
}