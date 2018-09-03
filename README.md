# DLogger
Derrick Kyle Ward's Asynchronous Logging Framework.

* **Purpose**:
  * Allows user to asynchronously Log Messages of different severity, from different threads, to a single log file. 
  
* **Why was this developed**:
  * This was my first stab at trying to develop a useful framework/library that can be used for any application. 
* **Challenges**:
  * Keeping the order in which the log messages are written to the file the same as the queueing order.
  * Making sure the thread working on the queue of messages does not remain the active thread in the application or on the system.
  * Providing an intuitive way for the user to halt the logging system and shut it down.
     * Using the method name C# developers would look for: Dispose()
     * This will ensure the logging system will take no more messages and the last queue messaged is written to the file.



* **Simple How To**:
```
//Create an instance of the logger.
IDLogger logger = new DLogger("Filename you want");

//Tell logger to log a info message, from the 'Program' Class Type
logger.Info("Info Severity Message", typeof(Program));

//Tell logger to log a warning message
logger.Warning("Warning Severity Message");

//Tell logger to log an error message
logger.Error("Error Severity Message");

//Tell Logger to stop listening for new messages and to shutdown
logger.Dispose();
```
