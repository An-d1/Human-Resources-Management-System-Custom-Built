namespace HRManagementSystem.Services
{
    public class CalculateLeaveDays
    {
        public int CalculateActualLeaveDays(DateTime start, DateTime end, string excludedDaysConfig)
        {
            // If the config is empty, count every single day
            if (string.IsNullOrWhiteSpace(excludedDaysConfig) || excludedDaysConfig == "None")
                return (end.Date - start.Date).Days + 1;

            var excluded = excludedDaysConfig.Split(',') // Converts the specified excluded days to .Net dayOfWeek object
                .Select(d => Enum.Parse<DayOfWeek>(d.Trim(), true))
                .ToList();

            int actualDays = 0;
            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                if (!excluded.Contains(date.DayOfWeek))
                {
                    actualDays++;
                }
            }
            return actualDays;
        }

        //method to handle year overlapping
        public int CalculateDaysInYear(DateTime reqStart, DateTime reqEnd, int year, string excludedDaysConfig)
        {
            // defining the start and end of the year jan 1, dec 31st
            var yearStart = new DateTime(year, 1, 1);
            var yearEnd = new DateTime(year, 12, 31);

            // Calculate the Intersection 
            // if the start year exceedes jan 1st thats the start date
            var actualStart = reqStart > yearStart ? reqStart : yearStart;

            // and if end of the year is smaller then dec 31, thats the end otherwise dec31 is
            var actualEnd = reqEnd < yearEnd ? reqEnd : yearEnd;

            // If there is no overlap, return 0
            if (actualStart > actualEnd) return 0;

            // then using the method above to calculate days with new start and end year
            return CalculateActualLeaveDays(actualStart, actualEnd, excludedDaysConfig);
        }
    }

}
