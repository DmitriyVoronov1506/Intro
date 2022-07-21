using System;

namespace WebApplicationClassWork.Services
{
    public class RandomService
    {
        private Random random = new Random();

        public int Integer => random.Next();
    }
}
