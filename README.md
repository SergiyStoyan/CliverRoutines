# CliverRoutines

## Overview
A cross-platform C# lib providing generally used routines: 
- application settings engine which replaces the Visual Studio application settings routine; 
- logging with multi-threading and session support;
- auxiliary routines;

### Framework
.NET Standard 2.0

### Supported platforms
It is supposed to run anywhere with .NET Standard lib supported. 
Presumably it will run on any Xamarin platforms (probably with minor updates required). 

The most concern would be peculiarities of the target file system because of writing/reading/managing files.

### Tested on:
- Windows 7, 10 in C# projects of any configuration built in Visual Studio;
- macOS High Sierra 10.12 in Xamarin.Mac projects built in Visual Studio for Mac;


## Application settings 
Classes: Cliver.Config, Cliver.Settings 

### Description
It is easy to use aplication settings engine which is much more powerful and flexible than the Visual Studio's one.

Features:
- automatically serialazing/deserialazing values of class members which need it;
- serializable types are tailored in the application according to needs;

### How to use:
Define types that are to be serialized and make them subclasses of Cliver.Settings class. Anywhere you want declare public fields of these types. Add the following call in the beginning of the app: 

Cliver.Config.Reload();

That's all. Now the settings fields are set with previously serialized values if any.

To serialize current value of a field, call Save() on it.

## Logging 
Classes: Cliver.Log

### Description
Writting logs on disk.

Features:
- thread-safe;
- (option) writting log per thread;
- (option) writting logs in sessions that an app can open and close many times during its work;
- (option) automatic old log cleanup; 

### How to use:
Add the following call in the beginning of the app: 

(optionally) Cliver.Log.Initialize();

To write to log, call either Cliver.Log.Write() or Cliver.Log.Main.Write() or more specific methods.

## Auxiliary routines 
### Description
Anything handy that is needed in general development.

### How to use:
See the code and annotations. 


## [More details...](https://sergeystoyan.github.io/CliverRoutines/#1)

Review my C# projects in github to see live examples.
