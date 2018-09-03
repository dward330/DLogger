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
            IDLogger logger = new DLogger("Shantel");

            logger.Info("Shantel Info");
            logger.Warning("Shantel Warning");
            logger.Error("Shantel Error");

            logger.Dispose();
        }
    }
}
