using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BCnEncoder.Shared
{
	public class AsyncOperation<T> : IDisposable
	{
		private CancellationTokenSource tokenSource;
		private Task<T> task;

		public T Result => task.Result;

		public Task<T> Start(Func<CancellationToken, T> func)
		{
			if (task != null)
			{
				throw new InvalidOperationException("This instance is already running an operation or finished it.");
			}

			var token = CreateTokenSource().Token;
			return task = Task.Factory.StartNew(() => func(token), token);
		}

		public void Cancel()
		{
			tokenSource.Cancel();
		}

		/// <summary>
		/// Allows awaiting the asynchronous operation.
		/// </summary>
		/// <returns>The awaiter for the running operation.</returns>
		/// <remarks>The operation should be started before awaiting it.</remarks>
		public TaskAwaiter<T>? GetAwaiter()
		{
			return task?.GetAwaiter();
		}

		public void Dispose()
		{
			tokenSource?.Dispose();
			task?.Dispose();
		}

		private CancellationTokenSource CreateTokenSource()
		{
			return tokenSource = new CancellationTokenSource();
		}
	}
}
