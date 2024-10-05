using System;

namespace ASI.Basecode.WebApp.Functions
{
    public class Helper
    {
        public static string GetTimeElapsed(TimeSpan timeElapsed, DateTime created_At)
        {
            string timeAgo = string.Empty;

            if (timeElapsed.TotalMinutes < 1)
            {
                timeAgo = "a few moments ago";
            }
            else if (timeElapsed.TotalMinutes < 60)
            {
                int minutes = (int)timeElapsed.TotalMinutes;
                timeAgo = minutes == 1 ? "1 minute ago" : $"{minutes} minutes ago";
            }
            else if (timeElapsed.TotalHours < 24)
            {
                int hours = (int)timeElapsed.TotalHours;
                timeAgo = hours == 1 ? "1 hour ago" : $"{hours} hours ago";
            }
            else if (timeElapsed.TotalDays < 7)
            {
                int days = (int)timeElapsed.TotalDays;
                timeAgo = days == 1 ? "1 day ago" : $"{days} days ago";
            }
            else
            {
                timeAgo = created_At.ToString("MMM dd, yyyy"); 
            }

            return timeAgo;
        }

    }
}
