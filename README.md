# Task Extensions

[![Build status]()]()

Several extensions to the awesome Task classes of the .NET Framework.

## Usage

This library features an ITaskScheduler interface that allows the creation of periodic- and one shot tasks.
Periodic tasks are meant to be used as a replacement for timers. Using periodic tasks allows for far better unit testing
compared to regular timers, thanks to the ManualTaskScheduler implementation.

void SomeMethod(ITaskScheduler scheduler)
{
	scheduler.StartPeriodic(() => Console.WriteLine("Hello World!"), TimeSpan.FromSeconds(1));
}

## Credits

Simon Mieﬂler 2017

## License

[MIT](http://opensource.org/licenses/MIT)
