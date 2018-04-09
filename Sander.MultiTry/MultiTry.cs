using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Sander.MultiTry
{
	public static class MultiTry
	{

		public static T Try<T>(Func<T> function, MultiTryOptions<T> options = null)
		{
			if (function == null)
				throw new ArgumentNullException(nameof(function), "MultiTry: function must be set!");

			if (options == null)
				options = MultiTryOptions<T>.Default;

			options.Validate();

			var i = 0;

			do
			{
				try
				{
					return function.Invoke();
				}
				catch (Exception ex) when (options.ExceptionFilter(ex))
				{
					Debug.WriteLine(ex);
					if (options.OnExceptionCallback?.Invoke(ex, i) == true)
						break;

					if (options.Delay > 0)
						Thread.Sleep(options.Delay);
				}

				i++;

			} while (i < options.TryCount);

			return options.OnFinalFailure.Invoke();
		}


		public static async Task<T> TryAsync<T>(Func<Task<T>> function, MultiTryOptions<T> options = null)
		{
			if (function == null)
				throw new ArgumentNullException(nameof(function), "MultiTry: function must be set!");

			if (options == null)
				options = MultiTryOptions<T>.Default;

			options.Validate();

			var i = 0;

			do
			{
				try
				{
					return await function.Invoke();
				}
				catch (Exception ex) when (options.ExceptionFilter(ex))
				{
					Debug.WriteLine(ex);
					if (options.OnExceptionCallback?.Invoke(ex, i) == true)
						break;

					if (options.Delay > 0)
						await Task.Delay(options.Delay);

				}

				i++;

			} while (i < options.TryCount);

			return options.OnFinalFailure.Invoke();
		}

	}
}
