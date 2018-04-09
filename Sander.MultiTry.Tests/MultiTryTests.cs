using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sander.MultiTry.Tests
{
	[TestClass]
	public class MultiTryTests
	{
		[TestMethod]
		public void BasicRun()
		{
			var options = MultiTryOptions<bool>.Default;

			options.OnExceptionCallback = (exception, i) =>
										  {
											  Trace.WriteLine($"{i}: {exception.Message}");
											  Assert.IsNotNull(exception);
											  Assert.IsTrue(i >= 0 && i < 3);
											  return false;
										  };
			options.Delay = 0;

			var result = MultiTry.Try(() => throw new NotImplementedException(), options);
			Assert.IsFalse(result);

		}

		[TestMethod]
		public void BasicRunAsync()
		{
			var options = MultiTryOptions<bool>.Default;

			options.OnExceptionCallback = (exception, i) =>
										  {
											  Trace.WriteLine($"{i}: {exception.Message}");
											  Assert.IsNotNull(exception);
											  Assert.IsTrue(i >= 0 && i < 3);
											  return false;
										  };
			options.Delay = 0;

			var func = new Func<Task<bool>>(async () =>
								{
									await Task.Delay(100);
									throw new NotImplementedException();
								});

			var result = MultiTry.TryAsync(func, options).Result;
			Assert.IsFalse(result);

		}
	}
}
