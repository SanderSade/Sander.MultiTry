using System;

namespace Sander.MultiTry
{
	public sealed class MultiTryOptions<T>
	{
		public Func<Exception, bool> ExceptionFilter { get; set; }

		public Func<Exception, int, bool> OnExceptionCallback { get; set; }

		public Func<T> OnFinalFailure { get; set; }

		public uint TryCount { get; set; } = 3;

		public int Delay { get; set; }


		public static MultiTryOptions<T> Default
		{
			get
			{
				var options = new MultiTryOptions<T>
				{
					ExceptionFilter = x => true,
					OnFinalFailure = () => default(T),
					TryCount = 3,
					Delay = 3000
				};

				return options;
			}
		}

		public void Validate()
		{
			if (ExceptionFilter == null)
				throw new ArgumentNullException($"{nameof(ExceptionFilter)}cannot be null!");


			if (OnFinalFailure == null)
				throw new ArgumentNullException($"{nameof(OnFinalFailure)}cannot be null!");

			if (Delay < 0)
				throw new ArgumentException($"{nameof(Delay)} cannot be less than 0!");
		}
	}
}