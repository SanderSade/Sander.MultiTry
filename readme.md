[![GitHub license](https://img.shields.io/badge/licence-MPL%202.0-brightgreen.svg)](https://github.com/SanderSade/UrlShortener/blob/master/LICENSE)
[![NetStandard 2.0](https://img.shields.io/badge/-.NET%20Standard%202.0-green.svg)](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md)

Work in progress.

## Introduction


## Features


## Examples


## FAQ

* **How to re-throw the exception and keep the stack trace?**

Use [`ExceptionDispatchInfo.Capture()`](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.exceptionservices.exceptiondispatchinfo.capture?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev15.query%3FappId%3DDev15IDEF1%26l%3DEN-US%26k%3Dk(System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.7.1);k(DevLang-csharp)%26rd%3Dtrue&view=netframework-4.7.2):
```
var options = MultiTryOptions<int>.Default;
options.OnFinalFailure = ex =>
{
	ExceptionDispatchInfo.Capture(ex)?.Throw();
	return 42; //make compiler happy
};

var result = MultiTry.Try(() => MyMethod(1, "a", false), options);

```
* **How to use the exception filter?**

See the documentation for "[when in a catch statement](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/when#when-in-a-catch-statement)".

ExceptionFilter can be any expression or method which returns boolean. Note that an error inside the exception filter will not be handled, and will propagate to caller.

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
			return true;	
		default: //don't retry
			return false;
	}
};
```
* **How to implement exception-specific error handling?**


```
var options = MultiTryOptions<int>.Default;
options.OnExceptionCallback = (ex, i) =>
{
	Trace.WriteLine($"Attempt {i}: {ex.Message}");
	switch (ex)
	{
		case SqlException sql: //transient - wait and retry
			Tread.Sleep(i * 1000); //wait more and more before retrying
			return true;
		case AuthenticationException auth: //get new authentication and then retry
			UpdateAuthentication();
			return true;
		default: //don't retry
			return false;
	}
};
```

* **How to throw a specific unhandled exception when all attempts fail?**

Use OnFinalFailure:
```
var options = MultiTryOptions<int>.Default;
options.OnFinalFailure = ex => {
	throw new MyCustomApplicationException("Error");
	return 0; //makes compiler happy
}
```