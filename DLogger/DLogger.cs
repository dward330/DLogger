using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DLogger.DomainModels;
using DLogger.Services;

namespace DLogger
{
    public class DLogger : IDLogger
    {
        #region Fields

        private readonly Object _logQueueLock = new Object(); //Will be used for logging queue thread management
        private readonly Object _logDestinationLock = new Object(); //Will be used for log writing thread management
        private List<DLoggerMessage> _logQueue = new List<DLoggerMessage>();
        private String logFileName = "DLogger";
        private String logFileFullPath = null;
        private LogLevel _LogLevel = LogLevel.Info;
        private TextWriter _FileWriter = null;
        private int WaitTime_ForDequeueAllLogMessages = 10;
        private Thread LogMessageDequeueThread = null;
        private Boolean LoggerShuttingDown = false;
        private Boolean LoggerDequeuerOffline = false;

        public enum LogLevel { Error = 0, Warning = 1, Info = 2 };

        #endregion

        #region Contructors

        public DLogger()
        {
            this.StartUpLogger();
        }

        public DLogger(String logFileName = "DLogger")
        {
            this.logFileName = logFileName;
            this.StartUpLogger();
        }

        public DLogger(LogLevel logLevel, String logFileName = "DLogger")
        {
            this._LogLevel = logLevel;
            this.logFileName = logFileName;
            this.StartUpLogger();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Completes Start Up Operations: Creates log file, Dequeuing Thread, and Logs Start up Message
        /// </summary>
        private void StartUpLogger()
        {
            lock (this._logDestinationLock)
            {
                //Get Current Date and Time, in a specific format
                String timeNow = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

                //1st Preference: Create Absolute Filepath with DateTime in filename
                String tempLogFullPath = String.Format("{0}\\{1}_{2}.txt", Directory.GetCurrentDirectory(), this.logFileName, timeNow);

                //2nd Preference: Create Abosolute Filepath with DateTime and fresh Guid in filename
                String tempAlternativeLogFullPath = String.Format("{0}\\{1}_{2}_{3}.txt", Directory.GetCurrentDirectory(), this.logFileName, timeNow, new Guid());

                if (!File.Exists(tempLogFullPath))
                {
                    this._FileWriter = File.CreateText(tempLogFullPath);
                    this.logFileFullPath = tempLogFullPath;
                }
                else
                {
                    this._FileWriter = File.CreateText(tempAlternativeLogFullPath);
                    this.logFileFullPath = tempAlternativeLogFullPath;
                }
            }

            //Start Logging Queued up Messages
            this.LogMessageDequeueThread = new Thread(LogAndDequeueAllLogMessages);
            this.LogMessageDequeueThread.IsBackground = true;
            this.LogMessageDequeueThread.Start();

            //Log Start up Message
            this.QueueLogMessage("Logger has started and is running...", LogLevel.Info, typeof(DLogger));
        }

        /// Adds a Message to the Log Queue, to be logged.
        /// </summary>
        /// <param name="logMessage">message to be added to the queue</param>
        /// <param name="logLevel">level/severity of the message</param>
        /// <param name="classLogging">class that is logging this message</param>
        /// <returns>whether or not confirmation message was added to the log queue</returns>
        public Boolean QueueLogMessage(String logMessage, LogLevel logLevel = LogLevel.Info, Type classLogging = null)
        {
            Boolean tempMessageQueued = false;

            try
            {
                lock (this._logQueueLock)
                {
                    if (null != this._logQueue && !this.LoggerDequeuerOffline)
                    {
                        #region Add the message to the queue to be logged

                        DLoggerMessage tempDLoggerMessage = new DLoggerMessage()
                        {
                            MessageId = new Guid().ToString(),
                            MessageDateTimeStamp = DateTime.Now,
                            Message = logMessage,
                            LogLevel = logLevel,
                            ClassLogging = classLogging
                        };
                        this._logQueue.Add(tempDLoggerMessage);

                        #endregion

                        #region Verify the Message was added to the queue

                        if (
                            this._logQueue.Exists(
                                x => x.Message.Equals(logMessage, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            tempMessageQueued = true;
                        }

                        #endregion
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("{0} - {1}", "Error Adding Message to Log Queue!", e));
            }

            return tempMessageQueued;
        }

        /// <summary>
        /// Logs and Dequeues all Log Messages
        /// </summary>
        /// <param name="logLevel"></param>
        private void LogAndDequeueAllLogMessages()
        {
            try
            {
                while (true)
                {
                    lock (this._logQueueLock)
                    {
                        lock (this._logDestinationLock)
                        {
                            if (null != this._logQueue && !this.LoggerDequeuerOffline)
                            {
                                foreach (DLoggerMessage logMessage in this._logQueue)
                                {
                                    if (null != this._FileWriter && ((int)logMessage.LogLevel) <= (int)this._LogLevel)
                                    {
                                        //Logs next message in queue to file.
                                        this._FileWriter.WriteLine(String.Format("{0}-{1} -> {2}: {3}",
                                            ((null != logMessage.ClassLogging) ? logMessage.ClassLogging.Name : @"N/A"),
                                            logMessage.MessageDateTimeStamp.ToString("yyyy-MM-dd_HH-mm-ss"), logMessage.LogLevel.ToString(),
                                            logMessage.Message));

                                        this._FileWriter.Flush();
                                    }
                                }

                                //Dequeue all Messages
                                this._logQueue.Clear();
                            }

                            // Stop logging messages to file and clearing queue, if signal to shutdown is set or file pointer is lost.
                            if (this.LoggerShuttingDown && null != this._FileWriter)
                            {
                                this._FileWriter.Dispose();
                                this.LoggerDequeuerOffline = true;

                                //Break out of infinite loop
                                break;
                            }
                        }
                    }

                    // Place this thread to sleep, so other application or system threads can become active. 
                    Thread.Sleep(this.WaitTime_ForDequeueAllLogMessages);
                }
            }
            catch (Exception e)
            {
                String tmpMessage = String.Format("{0} caught an exception! \n {1}", (new StackTrace().GetFrame(0).GetMethod().Name), e);
                Console.WriteLine(tmpMessage);
                Debug.WriteLine(tmpMessage);
            }
        }

        /// <summary>
        /// Shutdown logger and release all the logger's resources
        /// </summary>
        public void Dispose()
        {
            this.LoggerShuttingDown = true;

            //Wait until we finished dequeuing messages
            while (!this.LoggerDequeuerOffline)
                Thread.Sleep(1000);

            //Stop the Thread Dequeuing Log Messages
            this.LogMessageDequeueThread.Abort();
            this.LogMessageDequeueThread.Join();

            this._logQueue = null;
            this.logFileName = null;
            this.logFileFullPath = null;
            this._FileWriter = null;
        }

        /// <summary>
        /// Logs Error Message
        /// </summary>
        /// <param name="logMessage">message to log</param>
        /// <param name="classLogging">class that is logging this message</param>
        /// <returns></returns>
        bool IDLogger.Error(string logMessage, Type classLogging)
        {
            return QueueLogMessage(logMessage, LogLevel.Error, classLogging);
        }

        /// <summary>
        /// Logs Warning Message
        /// </summary>
        /// <param name="logMessage">message to log</param>
        /// <param name="classLogging">class that is logging this message</param>
        /// <returns></returns>
        bool IDLogger.Warning(string logMessage, Type classLogging)
        {
            return QueueLogMessage(logMessage, LogLevel.Warning, classLogging);
        }

        /// <summary>
        /// Logs Info Message
        /// </summary>
        /// <param name="logMessage">message to log</param>
        /// <param name="classLogging">class that is logging this message</param>
        /// <returns></returns>
        bool IDLogger.Info(string logMessage, Type classLogging)
        {
            return QueueLogMessage(logMessage, LogLevel.Info, classLogging);
        }

        #endregion
    }
}
