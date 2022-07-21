using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplicationClassWork.Services
{
    public interface ICurrentDate // интерфейс с 2 методами (для даты и времени) и геттером
    {
        public DateTime dateTimeCurrent { get; }
        public string GetCurrentDate();
        public string GetCurrentTime();
        
    }
}
