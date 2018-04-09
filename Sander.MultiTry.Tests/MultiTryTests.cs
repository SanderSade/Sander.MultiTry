using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
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

			var result = MultiTry.Try(() => throw new ApplicationException(), options);
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
									throw new ApplicationException();
								});

			var result = MultiTry.TryAsync(func, options).Result;
			Assert.IsFalse(result);
		}


		[TestMethod]
		public void NullOptions()
		{
			var result = MultiTry.Try<bool>(() => throw new ApplicationException());
			Assert.IsFalse(result);
		}


		[ExpectedException(typeof(ApplicationException))]
		[TestMethod]
		public void ThrowOnSpecificCallback()
		{
			var options = MultiTryOptions<bool>.Default;
			options.OnExceptionCallback = (ex, i) =>
			                              {
				                              Trace.WriteLine($"{i}: {ex.Message}");

											  if (i == 1)
					                              ExceptionDispatchInfo.Capture(ex)?.Throw();

				                              return false;
			                              };

			var result = MultiTry.Try(() => throw new ApplicationException(), options);
			Assert.IsFalse(result);
		}


		[TestMethod]
		public void NullCallback()
		{
			var options = MultiTryOptions<bool>.Default;
			options.OnExceptionCallback = null;
			var result = MultiTry.Try(() => throw new ApplicationException(), options);
			Assert.IsFalse(result);
		}


		[TestMethod]
		public void DefaultResult()
		{
			var options = MultiTryOptions<int>.Default;
			options.OnFinalFailure = () => 42;

			var result = MultiTry.Try(() => throw new ApplicationException(), options);
			Trace.WriteLine(result);
			Assert.AreEqual(result, 42);
		}

		[ExpectedException(typeof(ApplicationException))]
		[TestMethod]
		public void ApplyFilter()
		{
			var options = MultiTryOptions<bool>.Default;
			options.ExceptionFilter = ex => ex.GetType() == typeof(NotImplementedException);

			var result = MultiTry.Try(() => throw new ApplicationException(), options);
			Assert.IsFalse(result);
		}


		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public void NullFunc()
		{
			MultiTry.Try<bool>(null);
		}


	}
}
