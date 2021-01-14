using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BCnEncoder.Shared
{
	/// <summary>
	/// An async operation with cancel capability.
	/// </summary>
	/// <typeparam name="T">The type the operation returns after it finished.</typeparam>
	public class AsyncOperation<T> : IDisposable
	{
		private CancellationTokenSource tokenSource;
		private Task<T> task;

		/// <summary>
		/// The result of the operation after it finished.
		/// </summary>
		public T Result => task.Result;

		/// <summary>
		/// Starts the given delegate asynchronously.
		/// </summary>
		/// <param name="func">The delegate to execute in this operation.</param>
		/// <returns>The awaitable task for this operation.</returns>
		/// <exception cref="InvalidOperationException">If an operation is already running.</exception>
		public Task<T> Start(Func<CancellationToken, T> func)
		{
			if (task != null)
			{
				throw new InvalidOperationException("This instance is already running an operation or finished it.");
			}

			var token = CreateTokenSource().Token;
			return task = Task.Factory.StartNew(() => func(token), token);
		}

		/// <summary>
		/// Cancels the currently running operation.
		/// </summary>
		/// <remarks>May only be called after invoking <see cref="Start"/>.</remarks>
		/// <exception cref="InvalidOperationException">If an operation is not running already.</exception>
		public void Cancel()
		{
			if (task == null)
			{
				throw new InvalidOperationException("No operation is currently running.");
			}

			tokenSource?.Cancel();
		}

		/// <summary>
		/// Allows awaiting the asynchronous operation.
		/// </summary>
		/// <returns>The awaiter for the running operation.</returns>
		/// <remarks>The operation should be started before awaiting it.</remarks>
		public TaskAwaiter<T> GetAwaiter()
		{
			if (task == null)
			{
				throw new InvalidOperationException("No operation is currently running.");
			}

			return task.GetAwaiter();
		}

		/// <inheritdoc cref="Dispose"/>
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
