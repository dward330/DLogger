using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DLogger.Services
{
    public interface IDLogger
    {
        Boolean Error(String logMessage, Type classLogging = null);
        Boolean Warning(String logMessage, Type classLogging = null);
        Boolean Info(String logMessage, Type classLogging = null);
        void Dispose();
    }
}
