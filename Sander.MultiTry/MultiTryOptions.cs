using System;

namespace Sander.MultiTry
{
	public sealed class MultiTryOptions<T>
	{
		public Func<Exception, bool> ExceptionFilter { get; set; }

		public Func<Exception, int, bool> OnExceptionCallback { get; set; }

		public Func<T> OnFinalFailure { get; set; } = () => default(T);

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
					Delay = 0
				};

				return options;
			}
		}

		public void Validate()
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