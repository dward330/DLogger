# DLogger
Derrick Kyle Ward's Asynchronous Logging Framework.

* **Purpose**:
  * Allows user to asynchronously Log Messages of different severity, from different threads, to a single log file. 
  
* **Why was this developed**:
  * This was my first stab at trying to develop a useful framework/library that can be used for any application. 
* **What was Challenging**:
  * Making sure all messages are guarenteed to be logged before a program, using this framework, exits.



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
