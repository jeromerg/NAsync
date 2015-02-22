using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NAsync;
using NUnit.Framework;
using TaskExtensions = NAsync.TaskExtensions;
#pragma warning disable 162

namespace NAsyncTests
{
    [TestFixture]
    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]
    [SuppressMessage("ReSharper", "RedundantCast")]
    public class FinallyTest
    {
        #region Util

        public class Ex1 : Exception
        {

        }

        public class Ex2 : Exception
        {

        }

        public interface IAfter
        {
            void NextAction();
            void NextActionWithInput(int resultOfFirst);
            string NextFunction();
            string NextFunctionWithInput(int resultOfFirst);
            void CatchExceptionHandler(Exception e);
            int CatchExceptionHandlerWithOutput(Exception e);
        }

        #endregion

        #region Finally: Task and finally Action
        [Test]
        public void Task_Finally_Action__Normal()
        {
            var mock = new Mock<IAfter>();
            Task task = SimpleTaskFactory.Run(() => { /*do nothing*/})
                .Finally(() => mock.Object.NextAction());

            task.Wait();

            mock.Verify(then => then.NextAction(), Times.Once);
        }

        [Test]
        public void Task_Finally_Action__CancelledByFirst()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            var syncer = new CatchTest.Syncer();

            var mock = new Mock<IAfter>();

            Task task = SimpleTaskFactory
                .Run(() =>
                {
                    syncer.Step(1);
                    syncer.Step(4);
                    token.ThrowIfCancellationRequested();
                }, token)
                .Finally(() => mock.Object.NextAction());

            syncer.Step(2);
            source.Cancel();
            syncer.Step(3);

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the exception");
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf<OperationCanceledException>(e.InnerException);
                mock.Verify(then => then.NextAction(), Times.Once);
                Assert.Pass();
            }

        }

        [Test]
        public void Task_Finally_Action__ExceptionInFirst()
        {
            var mock = new Mock<IAfter>();
            var thrownException = new Exception();
            Task task = SimpleTaskFactory.Run(() => { throw thrownException; })
                .Finally(() => mock.Object.NextAction());

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the exception");
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(thrownException, e.InnerException);
                mock.Verify(then => then.NextAction(), Times.Once);
                Assert.Pass();
            }

        }

        [Test]
        public void Task_Finally_Action__ExceptionInSecond()
        {
            var thrownException = new Exception();
            Task task = SimpleTaskFactory.Run(() => { })
                .Finally(() =>
                {
                    throw thrownException;
                });

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the exception");
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(thrownException, e.InnerException);
                Assert.Pass();
            }
        }

        [Test]
        public void Task_Finally_Action__ExceptionInFirstAndSecond()
        {
            var thrownException1 = new Exception();
            var thrownException2 = new Exception();
            Task task = SimpleTaskFactory.Run(() => { throw thrownException1; })
                .Finally(() =>
                {
                    throw thrownException2;
                });

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the exception");
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(thrownException2, e.InnerException);
                Assert.Pass();
            }

        }
        #endregion

        #region Finally: TaskT and finally Action
        [Test]
        public void TaskT_Finally_Action__Normal()
        {
            var mock = new Mock<IAfter>();
            Task<int> task = SimpleTaskFactory.Run(() => 12)
                .Finally(() => mock.Object.NextAction());

            task.Wait();

            mock.Verify(then => then.NextAction(), Times.Once);
        }

        [Test]
        public void TaskT_Finally_Action__CancelledByFirst()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            var syncer = new CatchTest.Syncer();

            var mock = new Mock<IAfter>();

            Task<int> task = SimpleTaskFactory
                .Run(() =>
                {
                    syncer.Step(1);
                    syncer.Step(4);
                    token.ThrowIfCancellationRequested();
                    return 12;
                }, token)
                .Finally(() => mock.Object.NextAction());

            syncer.Step(2);
            source.Cancel();
            syncer.Step(3);

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the exception");
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf<OperationCanceledException>(e.InnerException);
                mock.Verify(then => then.NextAction(), Times.Once);
                Assert.Pass();
            }

        }


        [Test]
        public void TaskT_Finally_Action__ExceptionInFirst()
        {
            var mock = new Mock<IAfter>();
            var thrownException = new Exception();
            Task<int> task = SimpleTaskFactory.Run(() =>
            {
                throw thrownException;

                return 12;
            })
                .Finally(() => mock.Object.NextAction());

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the exception");
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(thrownException, e.InnerException);
                mock.Verify(then => then.NextAction(), Times.Once);
                Assert.Pass();
            }

        }

        [Test]
        public void TaskT_Finally_Action__ExceptionInSecond()
        {
            var thrownException = new Exception();
            Task<int> task = SimpleTaskFactory.Run(() => 12)
                .Finally(() =>
                {
                    throw thrownException;
                });

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the exception");
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(thrownException, e.InnerException);
                Assert.Pass();
            }
        }

        [Test]
        public void TaskT_Finally_Action__ExceptionInFirstAndSecond()
        {
            var thrownException1 = new Exception();
            var thrownException2 = new Exception();
            Task<int> task = SimpleTaskFactory.Run(() =>
                {
                    throw thrownException1;
                 
                    return 12;
                })
                .Finally(() =>
                {
                    throw thrownException2;
                });

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the exception");
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(thrownException2, e.InnerException);
                Assert.Pass();
            }

        }
        #endregion

    }
}
