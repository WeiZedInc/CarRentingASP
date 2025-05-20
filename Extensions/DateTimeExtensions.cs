namespace CarRentingASP.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime EnsureUtc(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            else if (dateTime.Kind == DateTimeKind.Local)
                return dateTime.ToUniversalTime();

            return dateTime; // Already UTC
        }
    }
}