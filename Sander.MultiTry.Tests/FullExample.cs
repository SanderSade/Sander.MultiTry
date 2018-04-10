using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sander.MultiTry.Tests
{
	[TestClass]
	public sealed class FullExample
	{
		private Uri _uri;


		[TestMethod]
		public void Start()
		{
			var page = GetPage().GetAwaiter().GetResult();
			Assert.IsNotNull(page);
		}


		private async Task<string> GetPage()
		{
			//nonexisting address
			_uri = new Uri("http://bad.example.org");

			var options = MultiTryOptions<string>.Default;
			//set delay between attempts to 100 ms
			options.Delay = 100;

			//if we get any other exception besides WebException, we don't want to retry
			options.ExceptionFilter = ex =>
									  {
										  Trace.WriteLine($"Exception: {ex}");
										  //true means "handle exception", false "don't handle at all"
										  return ex.GetType() == typeof(WebException);
									  };

			//try up to 3 times
			options.TryCount = 3;

			//when we get to error handler, let's change URL to correct one
			options.OnException = (exception, i) =>
								  {
									  _uri = new Uri("http://example.org");
									  Trace.WriteLine($"Attempt {i}: {exception.Message}. Changing URL to {_uri}");

									  //returning true continues with next attempt, false breaks out of retry loop and executes OnFinalFailure immediately
									  return true;
								  };

			//if all attempts have failed, let's rethrow the exception, with proper stacktrace
			options.OnFinalFailure = ex =>
									 {
										 ExceptionDispatchInfo.Capture(ex)?.Throw();
										 return null; //make compiler happy
									 };

			var page = await MultiTry.TryAsync(() => DownloadPage(), options);
			Trace.WriteLine(page);
			return page;
		}


		private async Task<string> DownloadPage()
		{
			using (WebClient client = new WebClient())
			{
				return await client.DownloadStringTaskAsync(_uri);
			}
		}
	}
}
