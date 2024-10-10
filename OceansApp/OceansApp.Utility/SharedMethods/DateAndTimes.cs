
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

        public static int GetWorkingDaysInMonth(DateTime date)
        {
            int year = date.Year;
            int month = date.Month;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            int workingDays = 0;

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime currentDay = new DateTime(year, month, day);
                if (currentDay.DayOfWeek != DayOfWeek.Saturday && currentDay.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }
            }

            return workingDays;
        }

        public static DateTime GetEndOfPeriod(DateTime currentDate, int period)
        {
            if (period == 1) // Biweekly
            {
                if (currentDate.Day <= 15)
                {
                    return new DateTime(currentDate.Year, currentDate.Month, 15);
                }
                else
                {
                    return new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));
                }
            }
            else if (period == 2) // Monthly
            {
                return new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));
            }
            throw new ArgumentException("The paymentPeriod value is invalid.");
        }

    }

}
