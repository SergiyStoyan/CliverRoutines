# CliverRoutines

CliverRoutines is a cross-platform C# lib providing what is needed in the most applications:

- settings manager that supersedes .NET ConfigurationManager;
- logger with multi-threading and session support;
- auxiliary routines;

It has been developed on .NET Standard 2.0 and supposed to run on any platform. 

Tested on:
- Windows 7, 10 in C# projects of any configuration built in Visual Studio;
- macOS High Sierra 10.12 in Xamarin.Mac projects built in Visual Studio for Mac;

## Application settings manager
Cliver.Config is an application settings manager intended as a capable, yet simple in use replacement for .NET ConfigurationManager.

Features:
- cross-platform;
- natively enables setting types of any complexity and functionality;
- settings are easily modified directly in code;
- enables settings polymorphism;
- thread-safe;
- serializes data in JSON;

## Logger 
Cliver.Log is a logger designed with usability and scalability in mind.

Features:
- cross-platform;
- thread-safe;
- session oriented - an application can write multiple log sessions successively or simultaneously. It is helpful when an application performs multiple independent tasks;
- thread oriented - it can automatically write a log per thread;
- auto-cleanup of old logs;
- diagnostic output;

## Auxiliary routines 
Anything handy that is needed in general development.


## [Documentation](https://sergeystoyan.github.io/CliverRoutines/#1)

