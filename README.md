# Task Extensions

[![Build status](https://ci.appveyor.com/api/projects/status/t70n74bkqoyf7ktc?svg=true)](https://ci.appveyor.com/project/Kittyfisto/system-threading-tasks-extensions)
![AppVeyor tests](https://img.shields.io/appveyor/tests/Kittyfisto/system-threading-tasks-extensions.svg?color=%234CC61E)
[![NuGet](https://img.shields.io/nuget/dt/System.Threading.Extensions.svg)](http://nuget.org/packages/System.Threading.Extensions)
[![NuGet](https://img.shields.io/nuget/v/System.Threading.Extensions.svg)](http://nuget.org/packages/System.Threading.Extensions)

Several extensions to the awesome Task classes of the .NET Framework.

## Tasks

This library features an ITaskScheduler interface that allows the creation of periodic- and one shot tasks.
Periodic tasks are meant to be used as a replacement for timers. Using periodic tasks allows for far better unit testing
compared to regular timers, thanks to the ManualTaskScheduler implementation.

```csharp
void SomeMethod(ITaskScheduler scheduler)
{
	scheduler.StartPeriodic(() => Console.WriteLine("Hello World!"), TimeSpan.FromSeconds(1));
}
```

## Filesystem

This libary offers an [`IFilesystem`](src/System.Extensions/IO/IFilesystem.cs) interface which offers methods with an often identical syntax that .NET natively offers through
classes such as `File`, `FileInfo`, `DirectoryInfo`, etc... The purpose of this interface is to offer an indirection which allows simpler unit testing of code which interacts
with the filesystem. While production code will most likely use an instance of [`Filesystem`](src/System.Extensions/IO/Filesystem.cs), unit tests may either mock the
`IFilesystem` interface themselves (using Google.Moq or equivalent frameworks) or use [`InMemoryFileSystem`](src/System.Extensions/IO/InMemoryFilesystem.cs) which stores the contents
of the filesystem it represents in memory instead.  
The benefit of using this interface is two-fold:
- Classes which interact with the filesystem can be easily unit tested
- Tests which would interact with the real filesystem now interact with an InMemoryFileSystem, which isolates tests from each other, improving robustness *and* test performance

## Credits

Simon Mießler 2021

## License

[MIT](http://opensource.org/licenses/MIT)
