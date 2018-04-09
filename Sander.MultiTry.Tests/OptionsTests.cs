using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sander.MultiTry.Tests
{
	[TestClass]
	public sealed class OptionsTests
	{
		[TestMethod]
		public void OptionsCreate()
		{
			var options = new MultiTryOptions<bool>();

			Assert.IsNotNull(options);
		}


		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public void NullExceptionFilter()
		{
			var options = MultiTryOptions<bool>.Default;
			options.ExceptionFilter = null;

			options.Validate();

			Assert.IsNotNull(options);
		}


		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public void NullOnFinalFailure()
		{
			var options = MultiTryOptions<bool>.Default;
			options.OnFinalFailure = null;

			options.Validate();

			Assert.IsNotNull(options);
		}


		[ExpectedException(typeof(ArgumentException))]
		[TestMethod]
		public void NegativeDelay()
		{
			var options = MultiTryOptions<bool>.Default;
			options.Delay = -1;
			options.Validate();

			Assert.IsNotNull(options);
		}
	}
}
