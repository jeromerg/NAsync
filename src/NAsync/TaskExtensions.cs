using System;
using System.Threading;
using System.Threading.Tasks;
using NAsync.JetBrains.Annotations;

namespace NAsync
{
    /// <summary>
    ///     Inspired from Stephen Toub @Microsoft implementation:
    ///     http://blogs.msdn.com/b/pfxteam/archive/2010/11/21/10094564.aspx
    /// </summary>
    public static class TaskExtensions
    {
        #region Then extensions with following action/function

        [NotNull]
        public static Task Then([NotNull] this Task first,
            [NotNull] Action next,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            return Then(first, next, CancellationToken.None, taskScheduler);
        }

        [NotNull]
        public static Task Then([NotNull] this Task first,
            [NotNull] Action next,
            CancellationToken cancellationToken,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<object>();
            first.ContinueWith(delegate
            {
                // ReSharper disable once PossibleNullReferenceException
                if (first.IsFaulted) tcs.TrySetException(first.Exception.InnerExceptions);
                else if (first.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        next();
                        tcs.SetResult(null);
                    }
                    catch (Exception exc)
                    {
                        tcs.TrySetException(exc);
                    }
                }
            },
                taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        [NotNull]
        public static Task Then<T1>([NotNull] this Task<T1> first,
            [NotNull] Action<T1> next,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            return Then(first, next, CancellationToken.None, taskScheduler);
        }

        [NotNull]
        public static Task Then<T1>([NotNull] this Task<T1> first,
            [NotNull] Action<T1> next,
            CancellationToken cancellationToken,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<object>();
            first.ContinueWith(delegate
            {
                // ReSharper disable once PossibleNullReferenceException
                if (first.IsFaulted) tcs.TrySetException(first.Exception.InnerExceptions);
                else if (first.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        next(first.Result);
                        tcs.TrySetResult(null);
                    }
                    catch (Exception exc)
                    {
                        tcs.TrySetException(exc);
                    }
                }
            },
                taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        [NotNull]
        public static Task<T2> Then<T2>([NotNull] this Task first,
            [NotNull] Func<T2> next,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            return Then(first, next, CancellationToken.None, taskScheduler);
        }

        [NotNull]
        public static Task<T2> Then<T2>([NotNull] this Task first,
            [NotNull] Func<T2> next,
            CancellationToken cancellationToken,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<T2>();
            first.ContinueWith(delegate
            {
                // ReSharper disable once PossibleNullReferenceException
                if (first.IsFaulted) tcs.TrySetException(first.Exception.InnerExceptions);
                else if (first.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        T2 t = next();
                        tcs.TrySetResult(t);
                    }
                    catch (Exception exc)
                    {
                        tcs.TrySetException(exc);
                    }
                }
            },
                taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        [NotNull]
        public static Task<T2> Then<T1, T2>([NotNull] this Task<T1> first,
            [NotNull] Func<T1, T2> next,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            return Then(first, next, CancellationToken.None, taskScheduler);
        }

        [NotNull]
        public static Task<T2> Then<T1, T2>([NotNull] this Task<T1> first,
            [NotNull] Func<T1, T2> next,
            CancellationToken cancellationToken,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<T2>();
            first.ContinueWith(delegate
            {
                // ReSharper disable once PossibleNullReferenceException
                if (first.IsFaulted) tcs.TrySetException(first.Exception.InnerExceptions);
                else if (first.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        T2 t = next(first.Result);
                        tcs.TrySetResult(t);
                    }
                    catch (Exception exc)
                    {
                        tcs.TrySetException(exc);
                    }
                }
            },
                taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        #endregion

        #region Then extensions with following Task (unwrapping the inner Task)

        [NotNull]
        public static Task Then([NotNull] this Task first,
            [NotNull] Func<Task> next,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            return Then(first, next, CancellationToken.None, taskScheduler);
        }

        [NotNull]
        public static Task Then([NotNull] this Task first,
            [NotNull] Func<Task> next,
            CancellationToken cancellationToken,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<object>();
            first.ContinueWith(delegate
            {
                // ReSharper disable once PossibleNullReferenceException
                if (first.IsFaulted) tcs.TrySetException(first.Exception.InnerExceptions);
                else if (first.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        Task t = next();
                        if (t == null) tcs.TrySetCanceled();
                        else
                            t.ContinueWith(delegate
                            {
                                // ReSharper disable PossibleNullReferenceException
                                if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                                // ReSharper restore PossibleNullReferenceException
                                else if (t.IsCanceled) tcs.TrySetCanceled();
                                else tcs.TrySetResult(null);
                            },
                                taskScheduler ?? TaskScheduler.Default);
                    }
                    catch (Exception exc)
                    {
                        tcs.TrySetException(exc);
                    }
                }
            },
                taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        [NotNull]
        public static Task Then<T1>([NotNull] this Task<T1> first,
            [NotNull] Func<T1, Task> next,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            return Then(first, next, CancellationToken.None, taskScheduler);
        }

        [NotNull]
        public static Task Then<T1>([NotNull] this Task<T1> first,
            [NotNull] Func<T1, Task> next,
            CancellationToken cancellationToken,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<object>();
            first.ContinueWith(delegate
            {
                // ReSharper disable once PossibleNullReferenceException
                if (first.IsFaulted) tcs.TrySetException(first.Exception.InnerExceptions);
                else if (first.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        Task t = next(first.Result);
                        if (t == null) tcs.TrySetCanceled();
                        else
                            t.ContinueWith(delegate
                            {
                                // ReSharper disable PossibleNullReferenceException
                                if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                                // ReSharper restore PossibleNullReferenceException
                                else if (t.IsCanceled) tcs.TrySetCanceled();
                                else tcs.TrySetResult(null);
                            },
                                taskScheduler ?? TaskScheduler.Default);
                    }
                    catch (Exception exc)
                    {
                        tcs.TrySetException(exc);
                    }
                }
            },
                taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        [NotNull]
        public static Task<T2> Then<T2>([NotNull] this Task first,
            [NotNull] Func<Task<T2>> next,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            return Then(first, next, CancellationToken.None, taskScheduler);
        }

        [NotNull]
        public static Task<T2> Then<T2>([NotNull] this Task first,
            [NotNull] Func<Task<T2>> next,
            CancellationToken cancellationToken,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<T2>();
            first.ContinueWith(delegate
            {
                // ReSharper disable once PossibleNullReferenceException
                if (first.IsFaulted) tcs.TrySetException(first.Exception.InnerExceptions);
                else if (first.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        Task<T2> t = next();
                        if (t == null) tcs.TrySetCanceled();
                        else
                            t.ContinueWith(delegate
                            {
                                // ReSharper disable PossibleNullReferenceException
                                if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                                // ReSharper restore PossibleNullReferenceException
                                else if (t.IsCanceled) tcs.TrySetCanceled();
                                else tcs.TrySetResult(t.Result);
                            },
                                taskScheduler ?? TaskScheduler.Default);
                    }
                    catch (Exception exc)
                    {
                        tcs.TrySetException(exc);
                    }
                }
            },
                taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        [NotNull]
        public static Task<T2> Then<T1, T2>([NotNull] this Task<T1> first,
            [NotNull] Func<T1, Task<T2>> next,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            return Then(first, next, CancellationToken.None, taskScheduler);
        }

        [NotNull]
        public static Task<T2> Then<T1, T2>([NotNull] this Task<T1> first,
            [NotNull] Func<T1, Task<T2>> next,
            CancellationToken cancellationToken,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<T2>();
            first.ContinueWith(delegate
            {
                // ReSharper disable once PossibleNullReferenceException
                if (first.IsFaulted) tcs.TrySetException(first.Exception.InnerExceptions);
                else if (first.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        Task<T2> t = next(first.Result);
                        if (t == null) tcs.TrySetCanceled();
                        else
                            t.ContinueWith(delegate
                            {
                                // ReSharper disable PossibleNullReferenceException
                                if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                                // ReSharper restore PossibleNullReferenceException
                                else if (t.IsCanceled) tcs.TrySetCanceled();
                                else tcs.TrySetResult(t.Result);
                            },
                                taskScheduler ?? TaskScheduler.Default);
                    }
                    catch (Exception exc)
                    {
                        tcs.TrySetException(exc);
                    }
                }
            },
                taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        #endregion

        #region Catch

        /// <summary>
        /// Catches any Exception of type TException bubbling up from the Task and allow the user to filter out the exception.
        /// Remark: You cannot filter the OperationCanceledException thrown on task cancellation, if the the first Task has been built with the token (i.e. by calling Then(second, token)).
        /// 
        /// Be aware that successive .Catch&lt;Ex>() corresponds 
        /// to the following synchronous syntax:
        /// <example>
        ///  try
        ///  {
        ///      try
        ///      {
        ///          // SYNC TASK
        ///      }
        ///      catch (Ex1 e)
        ///      {
        ///          // FIRST CATCH BLOCK
        ///      }
        ///  }
        ///  catch (Ex2 e)
        ///  {
        ///      // SECOND CATCH, MAY CATCH THROWN EXCEPTION BY FIRST CATCH BLOCK
        ///  }
        /// </example>
        /// </summary>
        [NotNull]
        public static Task Catch<TException>([NotNull] this Task first,
            [NotNull] Action<TException> next,
            [CanBeNull] TaskScheduler taskScheduler = null)
            where TException : Exception
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<object>();
            first.ContinueWith(delegate
            {
                if (first.IsFaulted)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var concernedException = first.Exception.InnerException as TException;
                    if (concernedException == null)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        tcs.TrySetException(first.Exception.InnerExceptions); // not concerned -> forward   
                    }
                    else
                    {
                        // handle the exception
                        try
                        {
                            next(concernedException);
                            tcs.SetResult(null);
                        }
                        catch (Exception e)
                        {
                            tcs.TrySetException(e);
                        }
                    }
                }
                else if (first.IsCanceled) tcs.TrySetCanceled(); // forward
                else tcs.SetResult(null); // forward
            },
                taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        [NotNull]
        public static Task<T1> Catch<T1, TException>([NotNull] this Task<T1> first,
            [NotNull] Func<TException, T1> next,
            [CanBeNull] TaskScheduler taskScheduler = null)
            where TException : Exception
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<T1>();
            first.ContinueWith(delegate
            {
                if (first.IsFaulted)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var concernedException = first.Exception.InnerException as TException;
                    if (concernedException == null)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        tcs.TrySetException(first.Exception.InnerExceptions); // not concerned -> forward
                    }
                    else
                    {
                        // handle the exception
                        try
                        {
                            T1 result = next(concernedException);
                            tcs.SetResult(result);
                        }
                        catch (Exception e)
                        {
                            tcs.TrySetException(e);
                        }
                    }
                }
                else if (first.IsCanceled) tcs.TrySetCanceled(); // forward
                else tcs.TrySetResult(first.Result); // forward
            },
                taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        #endregion

        #region Finally

        [NotNull]
        public static Task Finally([NotNull] this Task first,
            [NotNull] Action next,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<object>();
            first.ContinueWith(delegate
            {
                // in any case
                Exception localException = null;
                try
                {
                    next();
                }
                catch (Exception e)
                {
                    localException = e;
                }

                // then forward
                if (localException != null) tcs.TrySetException(localException);
                // ReSharper disable once PossibleNullReferenceException
                else if (first.IsFaulted) tcs.TrySetException(first.Exception.InnerExceptions);
                else if (first.IsCanceled) tcs.TrySetCanceled();
                else tcs.TrySetResult(null);
            },
                taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        [NotNull]
        public static Task<T1> Finally<T1>([NotNull] this Task<T1> first,
            [NotNull] Action next,
            [CanBeNull] TaskScheduler taskScheduler = null)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<T1>();
            first.ContinueWith(delegate
            {
                // in any case
                Exception localException = null;
                try
                {
                    next();
                }
                catch (Exception e)
                {
                    localException = e;
                }

                // then forward
                if (localException != null) tcs.TrySetException(localException);
                // ReSharper disable once PossibleNullReferenceException
                else if (first.IsFaulted) tcs.TrySetException(first.Exception.InnerExceptions);
                else if (first.IsCanceled) tcs.TrySetCanceled();
                else tcs.TrySetResult(first.Result);
            },
                taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        #endregion
    }
}