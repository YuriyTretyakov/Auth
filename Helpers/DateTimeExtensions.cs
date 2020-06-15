using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColibriWebApi.Helpers
{
    public static class DateTimeExtensions
    {
        public static bool InRange(this DateTime dateToCheck, DateTime startDate, DateTime endDate)
        {
            return dateToCheck >= startDate && dateToCheck < endDate;
        }
    }
}
