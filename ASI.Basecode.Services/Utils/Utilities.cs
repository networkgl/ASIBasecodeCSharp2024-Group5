using System;

namespace ASI.Basecode.WebApp.Utils
{
    public class Utilities
    {
        public static class TimeZoneConverter
        {
            private static string timeZoneId = "Singapore Standard Time";
            private static TimeZoneInfo timeZone;

            static TimeZoneConverter()
            {
                try
                {
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                }
                catch (TimeZoneNotFoundException)
                {
                    // Handle if the specified time zone is not found
                    Console.WriteLine("Time zone ID not found: " + timeZoneId);
                    // Use a fallback time zone or default to UTC
                    timeZone = TimeZoneInfo.Local;
                }
                catch (InvalidTimeZoneException)
                {
                    // Handle if the time zone ID is invalid
                    Console.WriteLine("Invalid time zone ID: " + timeZoneId);
                    // Use a fallback time zone or default to UTC
                    timeZone = TimeZoneInfo.Local;
                }
                catch (Exception ex)
                {
                    // Handle other potential exceptions
                    Console.WriteLine("Error initializing time zone: " + ex.Message);
                    // Rethrow the exception to indicate initialization failure
                    throw;
                }
            }

            public static DateTime ConvertTimeZone(DateTime dateTimeUtc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, timeZone);
            }
        }
    }
}
