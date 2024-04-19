
namespace OceansApp.Utility.SharedMethods
{
    public static class DateAndTimes
    {
        public static double CalculateNumHours(string timeFromPa, string timeToPa)
        {
            TimeSpan timeFrom = TimeSpan.Parse(timeFromPa);
            TimeSpan timeTo = TimeSpan.Parse(timeToPa);
            TimeSpan difference = timeTo - timeFrom;

            int hours = difference.Hours;
            int minutes = difference.Minutes;
            double totalHours = hours + minutes / 60.0;

            return totalHours;
        }
    }
}
