using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplicationClassWork.Services
{
    public class CurrentDateUtc : ICurrentDate
    {
        public DateTime dateTimeCurrent => DateTime.UtcNow; // Наш геттер для utc

        public string GetCurrentDate()
        {
            return dateTimeCurrent.ToString("D"); // Только дата в другом формате
        }

        public string GetCurrentTime()
        {
            return dateTimeCurrent.ToString("T");  // Время utc
        } 
    }
}
