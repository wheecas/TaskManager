using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Job.Service
{
    class TestService : ITestService
    {
        public void TestPrintAsync()
        {
            Console.WriteLine($"Test Print:{DateTime.Now.ToString("T")}");
        }
    }
}
