# Task Extensions

[![Build status](https://ci.appveyor.com/api/projects/status/t70n74bkqoyf7ktc?svg=true)](https://ci.appveyor.com/project/Kittyfisto/system-threading-tasks-extensions)

Several extensions to the awesome Task classes of the .NET Framework.

## Usage

This library features an ITaskScheduler interface that allows the creation of periodic- and one shot tasks.
Periodic tasks are meant to be used as a replacement for timers. Using periodic tasks allows for far better unit testing
compared to regular timers, thanks to the ManualTaskScheduler implementation.

```csharp
void SomeMethod(ITaskScheduler scheduler)
{
	scheduler.StartPeriodic(() => Console.WriteLine("Hello World!"), TimeSpan.FromSeconds(1));
}
```

## Credits

Simon Mießler 2018

## License

[MIT](http://opensource.org/licenses/MIT)
