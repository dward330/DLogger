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
            IDLogger logger = new DLogger("Derrick");

            //Tell logger to log a info message, from the Program Class Type
            logger.Info("Derrick Info", typeof(Program));

            //Tell logger to log a warning message
            logger.Warning("Derrick Warning");

            //Tell logger to log an error message
            logger.Error("Derrick Error");

            //Tell Logger to stop listening for new messages and to shutdown
            logger.Dispose();
        }
    }
}
