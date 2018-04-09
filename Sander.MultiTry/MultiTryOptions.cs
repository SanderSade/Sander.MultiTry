using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Sander.MultiTry.Tests")]

namespace Sander.MultiTry
{
	/// <summary>
	/// Specify options for retry, including attempt count and delay between attempts
	/// <para>Use this for calls without a return value (Action).</para>
	/// </summary>
	public class MultiTryOptions
	{
		/// <summary>
		/// Exception filter allows to catch only specific exceptions, or implement different handling options for different exceptions
		/// <para>Note that exception thrown inside the filter will not be handled</para>
		/// <para>ExceptionFilter can be any predicate - e.g. expression or method call which returns boolean. This can be left null.</para>
		/// <para>See https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/when#when-in-a-catch-statement </para>
		/// <example>ex => ex.GetType() == typeof(SqlException)</example>
		/// </summary>
		public Func<Exception, bool> ExceptionFilter { get; set; }

		/// <summary>
		/// Optional callback to execute when exception occurs.
		/// <para>Use this for logging or exception-specific handling</para>
		/// <para>Return true to exit the retry loop and execute <see cref="OnFinalFailure"/> immediately.</para>
		/// <para>Exception is the current exception, int is 0-based attempt number</para>
		/// <example>(ex, i) => { Trace.WriteLine($"Attempt: {i}, exception: {ex.Message}"
		///		return false;
		/// });</example>
		/// </summary>
		public Func<Exception, int, bool> OnExceptionCallback { get; set; }

		/// <summary>
		/// How many times try the execution? Defaults to 3.
		/// <para>Note that attempt to execute will always be made, even if TryCount is 0.</para>
		/// </summary>
		public uint TryCount { get; set; } = 3;

		/// <summary>
		/// Delay in milliseconds between execution attempts. Defaults to 0 (no delay)
		/// </summary>
		public int Delay { get; set; }

		/// <summary>
		/// Action to execute on final failure. Optional
		/// Exception is the last exception thrown
		/// </summary>
		public Action<Exception> OnFinalFailure { get; set; }

		/// <summary>
		/// Get default values for <see cref="MultiTryOptions{T}"/>
		/// </summary>
		public static MultiTryOptions Default
		{
			get
			{
				var options = new MultiTryOptions
				{
					ExceptionFilter = x => true,
					TryCount = 3,
					Delay = 0
				};

				return options;
			}
		}


		/// <summary>
		/// Internal validation routine
		/// </summary>
		internal void Validate()
		{
			if (ExceptionFilter == null)
				ExceptionFilter = x => true;

			if (Delay < 0)
				throw new ArgumentException($"{nameof(Delay)} cannot be less than 0!");
		}
	}

	/// <inheritdoc />
	/// <summary>
	/// Specify options for retry, including attempt count and delay between attempts
	/// <para>Use this for calls with a return value (Func).</para>
	/// </summary>
	/// <typeparam name="T">Return type</typeparam>
	public sealed class MultiTryOptions<T> : MultiTryOptions
	{
		/// <summary>
		/// Function to execute when all retries fail. Required
		/// <para>The return value of this function will be returned as Try/TryAsync methods return value</para>
		/// <para>Exception is the last exception thrown</para>
		/// <para>Defaults to ex => default(T) (return the default value of <typeparam name="T">T</typeparam>)</para>
		/// </summary>
		public new Func<Exception, T> OnFinalFailure { get; set; } = ex => default(T);

		/// <summary>
		/// Get default values for <see cref="MultiTryOptions{T}"/>
		/// </summary>
		public new static MultiTryOptions<T> Default
		{
			get
			{
				var options = new MultiTryOptions<T>
				{
					ExceptionFilter = x => true,
					OnFinalFailure = ex => default(T),
					TryCount = 3,
					Delay = 0
				};

				return options;
			}
		}


		/// <summary>
		/// Internal validation routine
		/// </summary>
		internal new void Validate()
		{
			if (ExceptionFilter == null)
				ExceptionFilter = x => true;

			if (OnFinalFailure == null)
				throw new ArgumentNullException($"{nameof(OnFinalFailure)}cannot be null!");

			if (Delay < 0)
				throw new ArgumentException($"{nameof(Delay)} cannot be less than 0!");
		}
	}
}
