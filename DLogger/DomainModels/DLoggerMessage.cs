using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLogger.DomainModels
{
    public class DLoggerMessage
    {
        public String MessageId { get; set; }
        public DateTime MessageDateTimeStamp { get; set; }
        public String Message { get; set; }
        public DLogger.LogLevel LogLevel { get; set; }
        public Type ClassLogging { get; set; }
    }
}
