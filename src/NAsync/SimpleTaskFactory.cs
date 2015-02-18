using System;
using System.Threading;
using System.Threading.Tasks;
using NAsync.JetBrains.Annotations;

namespace NAsync
{
    /// <summary>
    /// Api simplification of TaskFactory following .Net 4.5 logic. See
    /// http://blogs.msdn.com/b/pfxteam/archive/2011/10/24/10229468.aspx
    /// The code mimic the implementation of all height Run() methods in .Net Framework 4.5:
    /// http://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs,045a746eb48cbaa9

    /// Documentation is Copy and Paste from .Net Framework 4.5.2
    /// </summary>
    public static class SimpleTaskFactory
    {
        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a Task handle for that work.
        /// </summary>
        /// <param name="action">The work to execute asynchronously</param>
        /// <returns>A Task that represents the work queued to execute in the ThreadPool.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="action"/> parameter was null.
        /// </exception>
        [NotNull]
        public static Task Run([NotNull] Action action)
        {
            return Run(action, CancellationToken.None);
        }

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a Task handle for that work.
        /// </summary>
        /// <param name="action">The work to execute asynchronously</param>
        /// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
        /// <returns>A Task that represents the work queued to execute in the ThreadPool.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="action"/> parameter was null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The <see cref="T:System.CancellationTokenSource"/> associated with <paramref name="cancellationToken"/> was disposed.
        /// </exception>
        [NotNull]
        public static Task Run([NotNull] Action action, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(action, cancellationToken);
        }

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a Task(TResult) handle for that work.
        /// </summary>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A Task(TResult) that represents the work queued to execute in the ThreadPool.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="function"/> parameter was null.
        /// </exception>
        [NotNull]
        public static Task<T> Run<T>([NotNull] Func<T> function)
        {
            return Run(function, CancellationToken.None);
        }

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a Task(TResult) handle for that work.
        /// </summary>
        /// <param name="function">The work to execute asynchronously</param>
        /// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
        /// <returns>A Task(TResult) that represents the work queued to execute in the ThreadPool.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="function"/> parameter was null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The <see cref="T:System.CancellationTokenSource"/> associated with <paramref name="cancellationToken"/> was disposed.
        /// </exception>
        [NotNull]
        public static Task<T> Run<T>([NotNull] Func<T> function, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(function, cancellationToken);
        }

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a proxy for the
        /// Task returned by <paramref name="function"/>.
        /// </summary>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A Task that represents a proxy for the Task returned by <paramref name="function"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="function"/> parameter was null.
        /// </exception>
        [NotNull]
        public static Task Run([NotNull] Func<Task> function)
        {
            return Run(function, CancellationToken.None);
        }

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a proxy for the
        /// Task returned by <paramref name="function"/>.
        /// </summary>
        /// <param name="function">The work to execute asynchronously</param>
        /// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
        /// <returns>A Task that represents a proxy for the Task returned by <paramref name="function"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="function"/> parameter was null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The <see cref="T:System.CancellationTokenSource"/> associated with <paramref name="cancellationToken"/> was disposed.
        /// </exception>
        [NotNull]
        public static Task Run([NotNull] Func<Task> function, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(function, cancellationToken).Unwrap();
        }

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a proxy for the
        /// Task(TResult) returned by <paramref name="function"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the proxy Task.</typeparam>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A Task(TResult) that represents a proxy for the Task(TResult) returned by <paramref name="function"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="function"/> parameter was null.
        /// </exception>
        [NotNull]
        public static Task<TResult> Run<TResult>([NotNull] Func<Task<TResult>> function)
        {
            return Run(function, CancellationToken.None);
        }

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a proxy for the
        /// Task(TResult) returned by <paramref name="function"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the proxy Task.</typeparam>
        /// <param name="function">The work to execute asynchronously</param>
        /// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
        /// <returns>A Task(TResult) that represents a proxy for the Task(TResult) returned by <paramref name="function"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="function"/> parameter was null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The <see cref="T:System.CancellationTokenSource"/> associated with <paramref name="cancellationToken"/> was disposed.
        /// </exception>
        [NotNull]
        public static Task<TResult> Run<TResult>([NotNull] Func<Task<TResult>> function, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(function, CancellationToken.None).Unwrap();
        }
    }
}
