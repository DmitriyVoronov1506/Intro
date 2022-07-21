using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplicationClassWork.Services
{
    public class CurrentDate : ICurrentDate
    {
        public DateTime dateTimeCurrent => DateTime.Now;  // Наш геттер

        public string GetCurrentDate()
        {         
            return dateTimeCurrent.ToShortDateString();  // Только дата, без времени
        }

        public string GetCurrentTime()
        {
            return dateTimeCurrent.ToString("T"); // Только время, без даты
        }

    }
}
