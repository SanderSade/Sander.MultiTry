[![GitHub license](https://img.shields.io/badge/licence-MPL%202.0-brightgreen.svg)](https://github.com/SanderSade/UrlShortener/blob/master/LICENSE)
[![NetStandard 2.0](https://img.shields.io/badge/-.NET%20Standard%202.0-green.svg)](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md)
[![NuGet v1.0.0](https://img.shields.io/badge/NuGet-v1.0.0-lightgrey.svg)](https://www.nuget.org/packages/Sander.MultiTry/)

## Introduction
There are great many operations which can have transient errors, such as network requests, database operations and many more. In case of a transient exception, the operation usually needs to be retried.

MultiTry is created to reduce the amount of boilerplate code that needs to be written for such operations. The goal was to have an easy-to-use, but yet flexible and powerful re-try mechanism, which supports exception filtering, handling and re-throwing - see examples below.


## Features
* Easy to use
* Supports both functions (returns result) and actions (no return value)
* Supports async functions
* Optional exception filtering
* Configurable number of retries
* Configurable delay between attempts
* .NET Standard 2.0, meaning MultiTry can be used with .NET Framework 4.6.1+, .NET Core 2.0 and more - see [here](https://github.com/dotnet/standard/blob/master/docs/versions.md) for detailed information.


## Examples
Simplest possible use case:  
`UserInfo userInfo = MultiTry.Try(() => GetUserInfo(userId))`

This executes method GetUserInfo() with the default settings, meaning:
* 3 attempts to execute GetUserInfo()
* No delay between the attempts
* If all retries get an exception, Try returns `default(UserInfo)` (null if UserInfo is a class).

A more complex example with exception filtering and logging:
```
var options = MultiTryOptions<UserInfo>.Default;

//delay 1000 ms between attempts
options.Delay = 1000;

//try up to five times
options.TryCount = 5;

//retry only if the exception is SqlException
options.ExceptionFilter = ex => ex.GetType() == typeof(SqlException);

//log exception information
options.OnException = (ex, i) =>
{
	Trace.WriteLine($"Attempt {i}: {ex}");
	return true; //continue attempts	
};

//if all retries fail, create a new user and return that
options.OnFinalFailure = ex =>
{
	Trace.WriteLine($"Creating a new user with id {userId}. Fetching user failed with exception {ex}.";
	return new UserInfo { UserId = userId, IsNew = true }; 
};

var userInfo = MultiTry.Try(() => GetUserInfo(userId), options);
```

There is also a full commented example in tests, [FullExample.cs](https://github.com/SanderSade/Sander.MultiTry/blob/master/Sander.MultiTry.Tests/FullExample.cs).


## FAQ

* **How to re-throw the exception and keep the stack trace?**

`MultiTry.Rethrow(ex)` re-throws the exception using the original stack trace:
```
var options = MultiTryOptions<int>.Default;
options.OnFinalFailure = ex =>
{
	MultiTry.Rethrow(ex);
	return 42; //make compiler happy
};

var result = MultiTry.Try(() => MyMethod(1, "a", false), options);
```
 `MultiTry.Rethrow(ex)` uses  [`ExceptionDispatchInfo.Capture()`](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.exceptionservices.exceptiondispatchinfo.capture?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev15.query%3FappId%3DDev15IDEF1%26l%3DEN-US%26k%3Dk(System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.7.1);k(DevLang-csharp)%26rd%3Dtrue&view=netframework-4.7.2) to preserve the original stack trace, unlike `throw ex;`.

* **How to use the exception filter?**

See the documentation for "[when in a catch statement](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/when#when-in-a-catch-statement)".

ExceptionFilter can be any expression or method which returns boolean. Note that an error inside the exception filter will not be handled, and will propagate to the caller.

Simple function or method:
```
options.ExceptionFilter = ex => ex.GetType() == typeof(SqlException); //try repeatedly only if exception is SqlException

options.ExceptionFilter = ex => FilterException(ex); //call method accepting Exception and returns boolean.
```

More complex function for filtering:
```
var options = MultiTryOptions<int>.Default;
options.ExceptionFilter = ex =>
{
	Trace.WriteLine($"Exception: {ex}");
	switch (ex)
	{
		case SqlException sql: //transient, retry
			return true;
		case SomeOtherTransientException sot:
			DoSomething(sot);
			return true;	
		default: //don't retry
			return false;
	}
};
```
* **How to implement exception-specific error handling?**


```
var options = MultiTryOptions<int>.Default;
options.OnException = (ex, i) =>
{
	Trace.WriteLine($"Attempt {i}: {ex.Message}");
	switch (ex)
	{
		case SqlException sql: //transient - wait and retry
			Tread.Sleep((i + 1) * 1000); //wait more and more before retrying
			return true;
		case AuthenticationException auth: //get new authentication and then retry
			UpdateAuthentication();
			return true;
		default: //don't retry
			return false;
	}
};
```

* **How to throw a specific exception when all attempts fail?**

Use OnFinalFailure:
```
var options = MultiTryOptions<int>.Default;
options.OnFinalFailure = ex =>
{
	throw new MyCustomApplicationException("Error");
	return 0; //makes compiler happy
}
```
* **Why use MultiTry over Microsoft Enterprise Library [Transient Fault Handling Block](https://msdn.microsoft.com/en-us/library/hh680934(v=pandp.50).aspx)?**
  - Enterprise Library is not available for .NET Core
  - No longer maintained or updated
  - Elephant when you need a butterfly
  - Complex to configure, overkill for most common scenarios

* **But what about [Polly](https://github.com/App-vNext/Polly)?**  
I recommend using Polly if you have more complex scenarios than MultiTry is intended to handle. That said, Polly can be complex to configure and pain to debug.
  
## Changelog
* 1.0.0 Initial release
* 1.0.1 Adding MultiTry.Rethrow() (re-throwing exception while keeping the original context)