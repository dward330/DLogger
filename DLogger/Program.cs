using DLogger.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLogger
{
    class Program
    {
        static void Main(string[] args)
        {
            IDLogger logger = new DLogger();

            logger.Error("This is the first Log Statement.");
        }
    }
}
