using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Sander.MultiTry
{
	public static class MultiTry
	{
		/// <summary>
		/// Retry function repeatedly. See <see cref="MultiTryOptions{T}"/> for specific options
		/// </summary>
		/// <typeparam name="T">Return type</typeparam>
		/// <param name="function">Function to execute. Required</param>
		/// <param name="options">Retry options. Can be null, see <see cref="MultiTryOptions{T}"/> for defaults</param>
		/// <returns><param name="function">function</param> result or value specified in <see cref="MultiTryOptions{T}.OnFinalFailure"/></returns>
		public static T Try<T>(Func<T> function, MultiTryOptions<T> options = null)
		{
			options = Validate(function, options);
			var i = 0;
			Exception lastException;

			do
			{
				try
				{
					return function.Invoke();
				}
				catch (Exception ex) when (options.ExceptionFilter(ex))
				{
					lastException = ex;

					if (options.OnException?.Invoke(ex, i) == false)
						break;
				}

				if (options.Delay > 0)
					Thread.Sleep(options.Delay);

				i++;
			} while (i < options.TryCount);

			return options.OnFinalFailure.Invoke(lastException);
		}


		/// <summary>
		/// Retry asynchronous function repeatedly. See <see cref="MultiTryOptions{T}"/> for specific options
		/// </summary>
		/// <typeparam name="T">Return type</typeparam>
		/// <param name="function">Async function to execute. Required</param>
		/// <param name="options">Retry options. Can be null, see <see cref="MultiTryOptions{T}"/> for defaults</param>
		/// <returns>function result or value specified in <see cref="MultiTryOptions{T}.OnFinalFailure"/></returns>
		public static async Task<T> TryAsync<T>(Func<Task<T>> function, MultiTryOptions<T> options = null)
		{
			options = Validate(function, options);

			var i = 0;
			Exception lastException;
			do
			{
				try
				{
					return await function.Invoke();
				}
				catch (Exception ex) when (options.ExceptionFilter(ex))
				{
					lastException = ex;

					if (options.OnException?.Invoke(ex, i) == false)
						break;
				}

				if (options.Delay > 0)
					await Task.Delay(options.Delay);

				i++;
			} while (i < options.TryCount);

			return options.OnFinalFailure.Invoke(lastException);
		}


		/// <summary>
		/// Retry action repeatedly
		/// <param name="action">Action to execute</param>
		/// <param name="options">Retry options. Can be null, see <see cref="MultiTryOptions"/> for defaults</param>
		/// </summary>
		public static void Try(Action action, MultiTryOptions options = null)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action), "MultiTry: action must be set!");

			if (options == null)
				options = MultiTryOptions.Default;

			options.Validate();

			var i = 0;
			Exception lastException;
			do
			{
				try
				{
					action.Invoke();
					return;
				}
				catch (Exception ex) when (options.ExceptionFilter(ex))
				{
					lastException = ex;

					if (options.OnException?.Invoke(ex, i) == false)
						break;
				}

				if (options.Delay > 0)
					Thread.Sleep(options.Delay);

				i++;
			} while (i < options.TryCount);

			options.OnFinalFailure?.Invoke(lastException);
		}

		/// <summary>
		/// Re-throw exception with the original state
		/// </summary>
		public static void Rethrow(Exception ex)
		{
			ExceptionDispatchInfo.Capture(ex)?.Throw();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static MultiTryOptions<T> Validate<T>(Delegate function, MultiTryOptions<T> options)
		{
			if (function == null)
				throw new ArgumentNullException(nameof(function), "MultiTry: function must be set!");

			if (options == null)
				options = MultiTryOptions<T>.Default;

			options.Validate();
			return options;
		}
	}
}
