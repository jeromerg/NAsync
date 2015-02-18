using System;
using System.Threading;
using System.Threading.Tasks;

namespace NAsync
{
    /// <summary>
    /// Inspired from Stephen Toub @Microsoft implementation: http://blogs.msdn.com/b/pfxteam/archive/2010/11/21/10094564.aspx
    /// </summary>
    public static class TaskExtensions
    {
        #region Then extensions without cancellation Token

        public static Task Then(this Task first, Action next, TaskScheduler taskScheduler = null)
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
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task Then(this Task first, Func<Task> next, TaskScheduler taskScheduler = null)
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
                        var t = next();
                        if (t == null) tcs.TrySetCanceled();
                        else t.ContinueWith(delegate
                        {
                            // ReSharper disable PossibleNullReferenceException
                            if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                            // ReSharper restore PossibleNullReferenceException
                            else if (t.IsCanceled) tcs.TrySetCanceled();
                            else tcs.TrySetResult(null);
                        }, taskScheduler ?? TaskScheduler.Default);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task Then<T1>(this Task<T1> first, Action<T1> next, TaskScheduler taskScheduler = null)
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
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task Then<T1>(this Task<T1> first, Func<T1, Task> next, TaskScheduler taskScheduler = null)
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
                        var t = next(first.Result);
                        if (t == null) tcs.TrySetCanceled();
                        else t.ContinueWith(delegate
                        {
                            // ReSharper disable PossibleNullReferenceException
                            if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                            // ReSharper restore PossibleNullReferenceException
                            else if (t.IsCanceled) tcs.TrySetCanceled();
                            else tcs.TrySetResult(null);
                        }, taskScheduler ?? TaskScheduler.Default);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task<T2> Then<T2>(this Task first, Func<T2> next, TaskScheduler taskScheduler = null)
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
                        var t = next();
                        tcs.TrySetResult(t);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task<T2> Then<T2>(this Task first, Func<Task<T2>> next, TaskScheduler taskScheduler = null)
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
                        else t.ContinueWith(delegate
                        {
                            // ReSharper disable PossibleNullReferenceException
                            if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                            // ReSharper restore PossibleNullReferenceException
                            else if (t.IsCanceled) tcs.TrySetCanceled();
                            else tcs.TrySetResult(t.Result);
                        }, taskScheduler ?? TaskScheduler.Default);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task<T2> Then<T1, T2>(this Task<T1> first, Func<T1, T2> next, TaskScheduler taskScheduler = null)
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
                        var t = next(first.Result);
                        tcs.TrySetResult(t);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task<T2> Then<T1, T2>(this Task<T1> first, Func<T1, Task<T2>> next, TaskScheduler taskScheduler = null)
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
                        else t.ContinueWith(delegate
                        {
                            // ReSharper disable PossibleNullReferenceException
                            if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                            // ReSharper restore PossibleNullReferenceException
                            else if (t.IsCanceled) tcs.TrySetCanceled();
                            else tcs.TrySetResult(t.Result);
                        }, taskScheduler ?? TaskScheduler.Default);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        #endregion

        #region Then extensions WITH cancellation Token

        public static Task<T2> Then<T1, T2>(this Task<T1> first, Func<T1, CancellationToken, Task<T2>> next, CancellationToken token, TaskScheduler taskScheduler = null)
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
                        Task<T2> t = next(first.Result, token);
                        if (t == null) tcs.TrySetCanceled();
                        else t.ContinueWith(delegate
                        {
                            // ReSharper disable PossibleNullReferenceException
                            if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                            // ReSharper restore PossibleNullReferenceException
                            else if (t.IsCanceled) tcs.TrySetCanceled();
                            else tcs.TrySetResult(t.Result);
                        }, taskScheduler ?? TaskScheduler.Default);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task Then(this Task first, Action<CancellationToken> next, CancellationToken token, TaskScheduler taskScheduler = null)
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
                        next(token);
                        tcs.SetResult(null);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task Then(this Task first, Func<CancellationToken, Task> next, CancellationToken token, TaskScheduler taskScheduler = null)
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
                        var t = next(token);
                        if (t == null) tcs.TrySetCanceled();
                        else t.ContinueWith(delegate
                        {                            
                            // ReSharper disable PossibleNullReferenceException
                            if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                            // ReSharper restore PossibleNullReferenceException
                            else if (t.IsCanceled) tcs.TrySetCanceled();
                            else tcs.TrySetResult(null);
                        }, taskScheduler ?? TaskScheduler.Default);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task<T2> Then<T2>(this Task first, Func<CancellationToken, T2> next, CancellationToken token, TaskScheduler taskScheduler = null)
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
                        var t = next(token);
                        tcs.TrySetResult(t);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task<T2> Then<T2>(this Task first, Func<CancellationToken, Task<T2>> next, CancellationToken token, TaskScheduler taskScheduler = null)
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
                        Task<T2> t = next(token);
                        if (t == null) tcs.TrySetCanceled();
                        else t.ContinueWith(delegate
                        {
                            // ReSharper disable PossibleNullReferenceException
                            if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                            // ReSharper restore PossibleNullReferenceException
                            else if (t.IsCanceled) tcs.TrySetCanceled();
                            else tcs.TrySetResult(t.Result);
                        }, taskScheduler ?? TaskScheduler.Default);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task Then<T1>(this Task<T1> first, Action<T1, CancellationToken> next, CancellationToken token, TaskScheduler taskScheduler = null)
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
                        next(first.Result, token);
                        tcs.TrySetResult(null);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task Then<T1>(this Task<T1> first, Func<T1, CancellationToken, Task> next, CancellationToken token, TaskScheduler taskScheduler = null)
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
                        var t = next(first.Result, token);
                        if (t == null) tcs.TrySetCanceled();
                        else t.ContinueWith(delegate
                        {
                            // ReSharper disable PossibleNullReferenceException
                            if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                            // ReSharper restore PossibleNullReferenceException
                            else if (t.IsCanceled) tcs.TrySetCanceled();
                            else tcs.TrySetResult(null);
                        }, taskScheduler ?? TaskScheduler.Default);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task<T2> Then<T1, T2>(this Task<T1> first, Func<T1, CancellationToken, T2> next, CancellationToken token, TaskScheduler taskScheduler = null)
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
                        var t = next(first.Result, token);
                        tcs.TrySetResult(t);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }
        #endregion

        #region Catch

        public static Task Catch<TException>(this Task first, Action<TException> next, TaskScheduler taskScheduler = null)
            where TException : Exception
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<object>();
            first.ContinueWith(delegate
            {
                if (first.IsFaulted)
                {
                    var concernedException = first.Exception as TException;
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
                        }
                        catch (Exception e)
                        {
                            tcs.TrySetException(e);
                        }
                    }
                }
                else if (first.IsCanceled) tcs.TrySetCanceled();    // forward
                else tcs.SetResult(null);                           // forward

            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task<T1> Catch<T1, TException>(this Task<T1> first, Func<TException, T1> next, TaskScheduler taskScheduler = null)
            where TException : Exception
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<T1>();
            first.ContinueWith(delegate
            {
                if (first.IsFaulted)
                {
                    var concernedException = first.Exception as TException;
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
                        }
                        catch (Exception e)
                        {
                            tcs.TrySetException(e);
                        }
                    }
                }
                else if (first.IsCanceled) tcs.TrySetCanceled();     // forward
                else tcs.TrySetResult(first.Result);                 // forward

            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        #endregion

        #region Finally

        public static Task Finally(this Task first, Action next, TaskScheduler taskScheduler = null)
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
            
            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task<T1> Finally<T1>(this Task<T1> first, Action<T1> next, TaskScheduler taskScheduler = null)
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
                    next(first.Result);
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

            }, taskScheduler ?? TaskScheduler.Default);
            return tcs.Task;
        }

        #endregion

    }
}