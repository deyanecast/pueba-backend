using System;
using System.Globalization;

namespace MiBackend.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime ParseFlexible(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                throw new ArgumentException("La fecha no puede estar vacía");

            if (DateTime.TryParse(dateString, 
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out DateTime result))
            {
                return result;
            }

            throw new ArgumentException($"Formato de fecha no válido: {dateString}");
        }

        public static DateTime ToStartOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        }

        public static DateTime ToEndOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999, DateTimeKind.Utc);
        }
    }
} 